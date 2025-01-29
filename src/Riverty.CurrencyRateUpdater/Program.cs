using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Riverty.ExchangeRateCalculator.Services;
using ScheduledRateUpdater.Data;
using ScheduledRateUpdater.Services;

namespace ScheduledRateUpdater
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<RateDbContext>();
                dbContext.Database.Migrate();
            }

            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath);
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddDbContext<RateDbContext>(options =>
                        options.UseNpgsql(hostContext.Configuration.GetConnectionString("PostgreSQLConnection")));
                    services.AddSingleton<CurrencyService>();
                    services.AddScoped<RateUpdateService>();
                    services.AddHostedService<ScheduledRateUpdateBackgroundService>();
                });
    }
}