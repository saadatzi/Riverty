namespace Riverty.Web.Models
{
    public class RateResponseModel
    {
        public string? SourceCurrency { get; set; }
        public string? TargetCurrency { get; set; }
        public string? DateType { get; set; }
        public string? Date { get; set; }
        public decimal? ConvertedAmount { get; set; }
    }
}