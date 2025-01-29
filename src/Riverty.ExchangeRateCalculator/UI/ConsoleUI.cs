namespace Riverty.ExchangeRateCalculator.UI;

public static class ConsoleUI
{
    public static string? SelectItem(string prompt, string[] currencies)
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

    public static string? GetHistoricalDateInput(string prompt)
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

    public static void RedrawList(string[] items, int selectedIndex)
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