using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScheduledRateUpdater.Entities
{
    public class ExchangeRate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(3)]
        public string BaseCurrency { get; set; } = "EUR";
        public decimal USD { get; set; }
        public decimal AUD { get; set; }
        public decimal CAD { get; set; }
        public decimal PLN { get; set; }
        public decimal MXN { get; set; }

        [Required]
        public DateTime RateDate { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}