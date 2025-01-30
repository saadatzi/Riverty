using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Riverty.ExchangeRateCalculator.Services;
using Riverty.Web.Models;
using System.Diagnostics;
using System.Text.Json;
using static Riverty.ExchangeRateCalculator.Services.CurrencyService;

namespace Riverty.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IHttpClientFactory _clientFactory;
    private readonly ApiSettings _apiSettings;
    private readonly CurrencyService _currencyService;

    public HomeController(
        ILogger<HomeController> logger,
        IHttpClientFactory clientFactory,
        IOptions<ApiSettings> apiSettings,
        CurrencyService currencyService)
    {
        _logger = logger;
        _clientFactory = clientFactory;
        _apiSettings = apiSettings.Value;
        _currencyService = currencyService;
    }
    public IActionResult HistoricalRates()
    {
        var viewModel = new HistoricalRatesViewModel
        {
            HistoricalRates = new List<HistoricalRateData>()
        };
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> HistoricalRates(HistoricalRatesViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            using var client = _clientFactory.CreateClient();
            var startDateFormatted = model.StartDate?.ToString("yyyy-MM-dd");
            var endDateFormatted = model.EndDate?.ToString("yyyy-MM-dd");

            string apiUrl = $"{_apiSettings.BaseUrl}/historical-rates/{model.CurrencyCode}?startDate={startDateFormatted}&endDate={endDateFormatted}";

            var httpResponse = await client.GetAsync(apiUrl);
            httpResponse.EnsureSuccessStatusCode();

            var content = await httpResponse.Content.ReadAsStringAsync();
            var historicalRates = JsonSerializer.Deserialize<List<HistoricalRateData>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            model.HistoricalRates = historicalRates;

            return View(model);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error calling API");
            ModelState.AddModelError("", "Error retrieving data from the API.");
            return View(model);
        }
    }

    public IActionResult Index()
    {
        var viewModel = new RateViewModel
        {
            Request = new RateRequestModel { DateType = "Latest" }
        };
        return View(viewModel);
    }


    [HttpPost]
    public async Task<IActionResult> Index(RateViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }
        
        RateRequestModel model = viewModel.Request;

        try
            {
                decimal? convertedAmount;
                string? date = null;

                // Use CurrencyService.PerformConversion for both Latest and Historical
                if (model.DateType == "Historical")
                {
                    date = model.Date?.ToString("yyyy-MM-dd");
                }

                using var client = _clientFactory.CreateClient();

                string apiUrl = $"{_apiSettings.BaseUrl}/rates?dateType={model.DateType}&date={date}";

                var httpResponse = await client.GetAsync(apiUrl);
                httpResponse.EnsureSuccessStatusCode();

                var content = await httpResponse.Content.ReadAsStringAsync();
                var rates = JsonSerializer.Deserialize<ExchangeRateResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                convertedAmount = await PerformConversion(
                    model.SourceCurrency,
                    model.TargetCurrency,
                    model.Amount,
                    model.DateType!,
                    date,
                    rates
                );

                viewModel.Response = new RateResponseModel
                {
                    SourceCurrency = model.SourceCurrency,
                    TargetCurrency = model.TargetCurrency,
                    DateType = model.DateType,
                    Date = date,
                    ConvertedAmount = convertedAmount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during currency conversion");
                ModelState.AddModelError("Api Error", "An error occurred while performing the currency conversion.");
            }

            return View(viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}