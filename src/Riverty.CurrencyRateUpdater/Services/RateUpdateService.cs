using Microsoft.Extensions.Logging;
using Riverty.ExchangeRateCalculator.Services;
using ScheduledRateUpdater.Data;
using ScheduledRateUpdater.Entities;

namespace ScheduledRateUpdater.Services
{
    public class RateUpdateService
    {
        private readonly ILogger<RateUpdateService> _logger;
        private readonly CurrencyService _currencyService;
        private readonly RateDbContext _dbContext;

        public RateUpdateService(ILogger<RateUpdateService> logger, CurrencyService currencyService, RateDbContext dbContext)
        {
            _logger = logger;
            _currencyService = currencyService;
            _dbContext = dbContext;
        }

        public async Task UpdateRatesAsync()
        {
            _logger.LogInformation("Updating exchange rates...");

            try
            {
                // Get the latest rates from the API
                var exchangeRateResponse = await _currencyService.GetExchangeRatesAsync("Latest");

                if (exchangeRateResponse != null)
                {
                    // Map the response to the ExchangeRate entity
                    var exchangeRate = new ExchangeRate
                    {
                        BaseCurrency = exchangeRateResponse.BaseCurrency,
                        RateDate = exchangeRateResponse.RateDate,
                        Timestamp = DateTime.UtcNow,
                        USD = exchangeRateResponse.Rates.ContainsKey("USD") ? exchangeRateResponse.Rates["USD"] : 0,
                        AUD = exchangeRateResponse.Rates.ContainsKey("AUD") ? exchangeRateResponse.Rates["AUD"] : 0,
                        CAD = exchangeRateResponse.Rates.ContainsKey("CAD") ? exchangeRateResponse.Rates["CAD"] : 0,
                        PLN = exchangeRateResponse.Rates.ContainsKey("PLN") ? exchangeRateResponse.Rates["PLN"] : 0,
                        MXN = exchangeRateResponse.Rates.ContainsKey("MXN") ? exchangeRateResponse.Rates["MXN"] : 0,
                    };

                    _dbContext.ExchangeRates.Add(exchangeRate);

                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Exchange rates created successfully.");
                }
                else
                {
                    _logger.LogError("Failed to retrieve exchange rates from the API.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error created exchange rates");
            }
        }
    }
}