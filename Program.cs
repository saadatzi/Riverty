using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public class Program
{
    private static readonly string ApiKey = "f5c6066a8fa151f2a748f3defa92fd85"; 
    private static readonly string BaseUrl = "http://data.fixer.io/api/";
    private static readonly string BaseCurrency = "EUR"; 
    private static readonly string AllowedTargetCurrenciesString = "USD,AUD,CAD,PLN,MXN"; 
    private static readonly string[] AllowedTargetCurrencies = AllowedTargetCurrenciesString.Split(',');


    public static async Task Main(string[] args)
    {
        Console.WriteLine("Currency Converter - Latest Rates (Base Currency: EUR)");

        Console.Write("Enter source currency code (EUR, USD, AUD, CAD, PLN, MXN): ");
        string? sourceCurrency = Console.ReadLine()?.ToUpper();
        if (string.IsNullOrWhiteSpace(sourceCurrency) || !IsValidCurrencyCode(sourceCurrency))
        {
            Console.WriteLine("Invalid source currency code. Allowed : EUR, USD, AUD, CAD, PLN, MXN.");
            return;
        }

        Console.Write("Enter target currency code (EUR, USD, AUD, CAD, PLN, MXN): ");
        string? targetCurrency = Console.ReadLine()?.ToUpper();
        if (string.IsNullOrWhiteSpace(targetCurrency) || !IsValidCurrencyCode(targetCurrency))
        {
            Console.WriteLine("Invalid target currency code. Allowed: EUR, USD, AUD, CAD, PLN, MXN.");
            return;
        }

        Console.Write("Enter amount to convert: ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal amount) || amount <= 0)
        {
            Console.WriteLine("Invalid amount.");
            return;
        }

        using HttpClient client = new HttpClient();
        try
        {
            string latestRatesEndpoint = $"{BaseUrl}latest?access_key={ApiKey}&symbols={AllowedTargetCurrenciesString}&format=1";

            HttpResponseMessage response = await client.GetAsync(latestRatesEndpoint);
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

    private static bool IsValidCurrencyCode(string code)
    {
        if (code == BaseCurrency) return true; 
        foreach (var validCode in AllowedTargetCurrencies)
        {
            if (code == validCode) return true;
        }
        return false;
    }
}