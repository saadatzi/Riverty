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
        string consoleAggregateEachLevel = string.Empty;
        Console.WriteLine("Currency Converter - Latest Rates (Base Currency: EUR)");

        string? sourceCurrency = SelectItem("Select source currency:", AllowedTargetCurrencies);
        if (string.IsNullOrEmpty(sourceCurrency)) return;
        consoleAggregateEachLevel += $"Source currency: {sourceCurrency} \n";
        Console.Clear();
        Console.WriteLine(consoleAggregateEachLevel); 

        string? targetCurrency = SelectItem("Select target currency:", AllowedTargetCurrencies);
        if (string.IsNullOrEmpty(targetCurrency)) return;

        consoleAggregateEachLevel += $"Target currency: {targetCurrency} \n";
        Console.Clear(); 
        Console.WriteLine(consoleAggregateEachLevel);

        string? dateType = SelectItem("Select rate type:", new[] { "Latest", "Historical" });
        if (string.IsNullOrEmpty(dateType)) return;
        consoleAggregateEachLevel += $"Rate type: {dateType} \n";
        Console.Clear(); 
        Console.WriteLine(consoleAggregateEachLevel);

        string? date = string.Empty;
        if (dateType == "Historical")
        {
            date = GetHistoricalDateInput("Enter date (YYYY-MM-DD):");
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

    private static async Task PerformConversion(
        string sourceCurrency,
        string targetCurrency,
        decimal amount,
        string dateType,
        string? date)
    {
        using HttpClient client = new HttpClient();
        try
        {
            string allowedTargetCurrenciesString = string.Join(",", AllowedTargetCurrencies.Where(c => c != "EUR"));
            string apiUrl = $"{BaseUrl}";
            if (dateType == "Historical" && !string.IsNullOrEmpty(date))
            {
                apiUrl += $"{date}?access_key={ApiKey}&symbols={allowedTargetCurrenciesString}&format=1"; // Historical Endpoint
            }
            else
            {
                apiUrl += $"latest?access_key={ApiKey}&symbols={allowedTargetCurrenciesString}&format=1"; // Latest Endpoint
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

    private static string? SelectItem(string prompt, string[] currencies)
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
                    RedrawList(currencies, selectedIndex); 
                    break;
                case ConsoleKey.DownArrow:
                    selectedIndex = Math.Min(currencies.Length - 1, selectedIndex + 1);
                    RedrawList(currencies, selectedIndex); 
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

    private static string? GetHistoricalDateInput(string prompt)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine();
            Console.WriteLine();
            Console.Write(prompt + " (YYYY-MM-DD, or leave empty to cancel): ");
            string? dateInput = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(dateInput))
            {
                return null; // User cancelled
            }

            if (DateTime.TryParseExact(dateInput, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out _))
            {
                return dateInput; // Valid date format
            }
            else
            {
                Console.WriteLine("Invalid date format. Please use YYYY-MM-DD.");
                Console.WriteLine("Press any key to try again.");
                Console.ReadKey(true);
            }
        }
    }

    private static void RedrawList(string[] items, int selectedIndex)
    {
        int startLine = Console.CursorTop - items.Length;
        if (startLine < 0) startLine = 0;

        Console.CursorTop = startLine;

        for (int i = 0; i < items.Length; i++)
        {
            Console.CursorLeft = 0;
            if (i == selectedIndex)
            {
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.WriteLine($"»»» {items[i]}");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"    {items[i]}");
            }
        }
    }
}