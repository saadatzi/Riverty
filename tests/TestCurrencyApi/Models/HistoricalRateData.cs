namespace TestCurrencyApi.Models;

public class HistoricalRateData
{
    public DateTime RateDate { get; set; }
    public Dictionary<string, decimal> Rates { get; set; } = new Dictionary<string, decimal>();
}