namespace TestCurrencyApi.Models;

public class ExchangeRateResponse
{
    public string BaseCurrency { get; set; } = string.Empty;
    public Dictionary<string, decimal> Rates { get; set; } = new Dictionary<string, decimal>();
    public DateTime RateDate { get; set; } = DateTime.UtcNow.Date;
}