using System;
using System.ComponentModel.DataAnnotations;

namespace Riverty.Web.Models
{
    public class RateRequestModel
    {
        [Required(ErrorMessage = "Source currency is required")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be a 3-letter string.")]
        public string? SourceCurrency { get; set; }

        [Required(ErrorMessage = "Target currency is required")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be a 3-letter string.")]
        public string? TargetCurrency { get; set; }

        public string? DateType { get; set; }

        [DataType(DataType.Date)]
        public DateTime? Date { get; set; }

        [Required(ErrorMessage = "Amount is required")]
         [Range(0.01, 10_000_000_000_000, ErrorMessage = "Please enter a valid amount greater than 0.")]
        public decimal Amount { get; set; }

    }
}