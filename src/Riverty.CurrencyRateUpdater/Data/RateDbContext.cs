using Microsoft.EntityFrameworkCore;
using ScheduledRateUpdater.Entities;

namespace ScheduledRateUpdater.Data
{
    public class RateDbContext : DbContext
    {
        public RateDbContext(DbContextOptions<RateDbContext> options) : base(options)
        {
        }

        public DbSet<ExchangeRate> ExchangeRates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}