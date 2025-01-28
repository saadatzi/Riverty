using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq; 

public class Program
{
    private static readonly string ApiKey = "f5c6066a8fa151f2a748f3defa92fd85"; // I will reset or invoke this key, so you may use yours
    private static readonly string BaseUrl = "http://data.fixer.io/api/";
    private static readonly string BaseCurrency = "EUR"; 
    private static readonly string[] AllowedTargetCurrencies = { "EUR", "USD", "AUD", "CAD", "PLN", "MXN" }; 

    public static async Task Main(string[] args)
    {
        Console.WriteLine("Currency Converter - Latest Rates (Base Currency: EUR)");

        string? sourceCurrency = SelectCurrency("Select source currency:", AllowedTargetCurrencies);
        if (string.IsNullOrEmpty(sourceCurrency)) return;

        Console.Clear(); 
        Console.WriteLine($"Source currency: {sourceCurrency}"); 

        string? targetCurrency = SelectCurrency("Select target currency:", AllowedTargetCurrencies);
        if (string.IsNullOrEmpty(targetCurrency)) return;

        Console.Clear(); 
        Console.WriteLine($"Source currency: {sourceCurrency}"); 
        Console.WriteLine($"Target currency: {targetCurrency}"); 

        Console.Write("Enter amount to convert: ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal amount) || amount <= 0)
        {
            Console.WriteLine("Invalid amount.");
            return;
        }

        await PerformConversion(sourceCurrency, targetCurrency, amount);
    }

    private static async Task PerformConversion(string sourceCurrency, string targetCurrency, decimal amount)
    {
        using HttpClient client = new HttpClient();
        try
        {
            string allowedTargetCurrenciesString = string.Join(",", AllowedTargetCurrencies.Where(c => c != "EUR")); 
            string latestRatesEndpoint = $"{BaseUrl}latest?access_key={ApiKey}&symbols={allowedTargetCurrenciesString}&format=1";

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

    private static string? SelectCurrency(string prompt, string[] currencies)
    {
        Console.WriteLine(); 
        Console.WriteLine(); 

        Console.CursorVisible = false;
        int selectedIndex = 0;

        Console.WriteLine(prompt);
        for (int i = 0; i < currencies.Length; i++) 
        {
            if (i == selectedIndex)
            {
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.WriteLine($"»»» {currencies[i]}");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"    {currencies[i]}");
            }
        }

        while (true)
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            switch (keyInfo.Key)
            {
                case ConsoleKey.UpArrow:
                    selectedIndex = Math.Max(0, selectedIndex - 1);
                    RedrawCurrencyList(currencies, selectedIndex); 
                    break;
                case ConsoleKey.DownArrow:
                    selectedIndex = Math.Min(currencies.Length - 1, selectedIndex + 1);
                    RedrawCurrencyList(currencies, selectedIndex); 
                    break;
                case ConsoleKey.Enter:
                    Console.CursorVisible = true;
                    Console.WriteLine(); 
                    return currencies[selectedIndex];
                case ConsoleKey.Escape:
                    Console.CursorVisible = true;
                    Console.WriteLine();
                    return null;
            }
        }
    }

    private static void RedrawCurrencyList(string[] currencies, int selectedIndex)
    {
        int startLine = Console.CursorTop - currencies.Length; 
        if (startLine < 0) startLine = 0; 

        Console.CursorTop = startLine; 

        for (int i = 0; i < currencies.Length; i++)
        {
            Console.CursorLeft = 0; 
            if (i == selectedIndex)
            {
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.WriteLine($"»»» {currencies[i]}");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"    {currencies[i]}");
            }
        }
    }
}