using System.Text.Json;
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

        await PerformConversion(sourceCurrency, targetCurrency, amount, dateType, date);
    }
    public async Task PerformConversion(
        string sourceCurrency,
        string targetCurrency,
        decimal amount,
        string dateType,
        string? date = default)
    {
        using HttpClient client = new HttpClient();
        try
        {
            string allowedTargetCurrenciesString = string.Join(",", AllowedTargetCurrencies.Where(c => c != "EUR"));
            string apiUrl = $"{_baseUrl}";
            if (dateType == "Historical" && !string.IsNullOrEmpty(date))
            {
                apiUrl += $"{date}?access_key={_apiKey}&symbols={allowedTargetCurrenciesString}&format=1"; // Historical Endpoint
            }
            else
            {
                apiUrl += $"latest?access_key={_apiKey}&symbols={allowedTargetCurrenciesString}&format=1"; // Latest Endpoint
            }

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();

            string jsonResponse = await response.Content.ReadAsStringAsync();
            JsonDocument document = JsonDocument.Parse(jsonResponse);

            if (document.RootElement.TryGetProperty("rates", out JsonElement ratesElement))
            {
                decimal sourceRateToEur, targetRateToEur;

                if (sourceCurrency == BaseCurrency)
                {
                    sourceRateToEur = 1.0m;
                }
                else if (ratesElement.TryGetProperty(sourceCurrency, out JsonElement sourceRateElement) && sourceRateElement.TryGetDecimal(out sourceRateToEur))
                {
                    sourceRateToEur = 1.0m / sourceRateToEur; 
                }
                else
                {
                    Console.WriteLine($"Could not find exchange rate for source currency {sourceCurrency} against EUR.");
                    return;
                }

                if (targetCurrency == BaseCurrency)
                {
                    targetRateToEur = 1.0m;
                }
                else if (ratesElement.TryGetProperty(targetCurrency, out JsonElement targetRateElement) && targetRateElement.TryGetDecimal(out targetRateToEur))
                {
                    targetRateToEur = targetRateElement.GetDecimal();
                }
                else
                {
                    Console.WriteLine($"Could not find exchange rate for target currency {targetCurrency} against EUR.");
                    return;
                }

                decimal convertedAmount = amount * targetRateToEur * sourceRateToEur;
                Console.WriteLine($"\n{amount} {sourceCurrency} is equal to {convertedAmount:F2} {targetCurrency}");
            }
            else if (document.RootElement.TryGetProperty("error", out JsonElement errorElement))
            {
                Console.WriteLine($"API Error: {errorElement.GetRawText()}");
            }
            else
            {
                Console.WriteLine("Could not find exchange rates in the API response.");
                Console.WriteLine($"Raw response: {jsonResponse}");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"HTTP Request Error: {ex.Message}");
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON Parsing Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
        }
    }
}