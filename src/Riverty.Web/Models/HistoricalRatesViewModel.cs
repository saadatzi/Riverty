using System.ComponentModel.DataAnnotations;

namespace Riverty.Web.Models;

public class HistoricalRatesViewModel
{
    [Required(ErrorMessage = "Currency code is required")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be a 3-letter string.")]
    public string? CurrencyCode { get; set; }

    [DataType(DataType.Date)]
    public DateTime? StartDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }

    public List<HistoricalRateData>? HistoricalRates { get; set; }
}

public class HistoricalRateData
{
    public DateTime RateDate { get; set; }
    public Dictionary<string, decimal> Rates { get; set; } = new Dictionary<string, decimal>();
}