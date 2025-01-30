using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Riverty.ExchangeRateCalculator.UI;

namespace Riverty.ExchangeRateCalculator.Services;

public class CurrencyService
{

    private readonly string _apiKey;
    private readonly string _baseUrl;
    private static readonly string BaseCurrency = "EUR";
    private static readonly string[] AllowedTargetCurrencies = { "EUR", "USD", "AUD", "CAD", "PLN", "MXN" };

    public CurrencyService(IConfiguration configuration)
    {
        _apiKey = configuration["FixerApiKey"] ?? throw new InvalidOperationException("FixerApiKey is missing from configuration.");
        _baseUrl = configuration["FixerBaseUrl"] ?? "http://data.fixer.io/api/";
    }


    public async Task Start()
    {
        string consoleAggregateEachLevel = string.Empty;
        Console.WriteLine("Currency Converter - Latest Rates (Base Currency: EUR)");

        string? sourceCurrency = ConsoleUI.SelectItem("Select source currency:", AllowedTargetCurrencies);
        if (string.IsNullOrEmpty(sourceCurrency)) return;
        consoleAggregateEachLevel += $"Source currency: {sourceCurrency} \n";
        Console.Clear();
        Console.WriteLine(consoleAggregateEachLevel);

        string? targetCurrency = ConsoleUI.SelectItem("Select target currency:", AllowedTargetCurrencies);
        if (string.IsNullOrEmpty(targetCurrency)) return;

        consoleAggregateEachLevel += $"Target currency: {targetCurrency} \n";
        Console.Clear();
        Console.WriteLine(consoleAggregateEachLevel);

        string? dateType = ConsoleUI.SelectItem("Select rate type:", new[] { "Latest", "Historical" });
        if (string.IsNullOrEmpty(dateType)) return;
        consoleAggregateEachLevel += $"Rate type: {dateType} \n";
        Console.Clear();
        Console.WriteLine(consoleAggregateEachLevel);

        string? date = string.Empty;
        if (dateType == "Historical")
        {
            date = ConsoleUI.GetHistoricalDateInput("Enter date (YYYY-MM-DD):");
            if (string.IsNullOrEmpty(date)) return;
            consoleAggregateEachLevel += $"Date: {date} \n";
            Console.Clear();
            Console.WriteLine(consoleAggregateEachLevel);
        }

        Console.Write("Enter amount to convert: ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal amount) || amount <= 0)
        {
            Console.WriteLine("Invalid amount.");
            return;
        }

        var rates = await GetExchangeRatesAsync(dateType, date);
        await PerformConversion(sourceCurrency, targetCurrency, amount, dateType, date, rates);
    }
    public static Task<decimal?> PerformConversion(string sourceCurrency, string targetCurrency, decimal amount, string dateType, string? date, ExchangeRateResponse rates)
    {
        try
        {

            if (rates != null)
            {
                decimal sourceRateToEur, targetRateToEur;

                if (sourceCurrency == BaseCurrency)
                {
                    sourceRateToEur = 1.0m;
                }
                else if (rates.Rates.TryGetValue(sourceCurrency, out sourceRateToEur))
                {
                    sourceRateToEur = 1.0m / sourceRateToEur;
                }
                else
                {
                    Console.WriteLine($"Could not find exchange rate for source currency {sourceCurrency} against EUR.");
                }

                if (targetCurrency == BaseCurrency)
                {
                    targetRateToEur = 1.0m;
                }
                else if (rates.Rates.TryGetValue(targetCurrency, out targetRateToEur))
                {
                }
                else
                {
                    Console.WriteLine($"Could not find exchange rate for target currency {targetCurrency} against EUR.");
                }

                decimal convertedAmount = amount * targetRateToEur * sourceRateToEur;
                Console.WriteLine($"\n{amount} {sourceCurrency} is equal to {convertedAmount:F2} {targetCurrency} ({dateType} rate{(dateType == "Historical" ? $" on {date}" : "")})");
                return Task.FromResult<decimal?>(convertedAmount);
            }
            else
            {
                Console.WriteLine("Could not retrieve exchange rates.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
        }
        return Task.FromResult<decimal?>(null);

    }

    public async Task<ExchangeRateResponse> GetExchangeRatesAsync(string dateType, string? date = default)
    {
        using HttpClient client = new HttpClient();
        string allowedTargetCurrenciesString = string.Join(",", AllowedTargetCurrencies.Where(c => c != "EUR"));
        string apiUrl = $"{_baseUrl}";
        if (dateType == "Historical" && !string.IsNullOrEmpty(date))
        {
            apiUrl += $"{date}?access_key={_apiKey}&symbols={allowedTargetCurrenciesString}&format=1";
        }
        else
        {
            apiUrl += $"latest?access_key={_apiKey}&symbols={allowedTargetCurrenciesString}&format=1";
        }

        HttpResponseMessage response = await client.GetAsync(apiUrl);
        response.EnsureSuccessStatusCode();

        string jsonResponse = await response.Content.ReadAsStringAsync();
        JsonDocument document = JsonDocument.Parse(jsonResponse);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var result = JsonSerializer.Deserialize<FixerApiResponseModel>(jsonResponse, options);

        if (result is { Success: true })
        {
            return new ExchangeRateResponse
            {
                BaseCurrency = result.Base,
                Rates = result.Rates,
                RateDate = DateTime.UtcNow.Date
            };
        }
        else
        {
            // Handle error
            if (result?.Error != null)
            {
                Console.WriteLine($"API Error: {result.Error.Type} - {result.Error.Info}");
            }
            else
            {
                Console.WriteLine("API request failed with an unknown error.");
            }

            return null;
        }
    }
    
    public class ExchangeRateResponse
    {
        public string BaseCurrency { get; set; } = string.Empty;
        public Dictionary<string, decimal> Rates { get; set; } = new Dictionary<string, decimal>();
        public DateTime RateDate { get; set; }
    }

    public class FixerApiResponseModel
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

        [JsonPropertyName("base")]
        public string Base { get; set; } = string.Empty;

        [JsonPropertyName("date")]
        public string Date { get; set; } = string.Empty;

        [JsonPropertyName("rates")]
        public Dictionary<string, decimal> Rates { get; set; } = new Dictionary<string, decimal>();

        [JsonPropertyName("error")]
        public FixerApiError? Error { get; set; }
    }

    public class FixerApiError
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("info")]
        public string Info { get; set; } = string.Empty;
    }
}