using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using NCrontab;
using ScheduledRateUpdater.Services;

namespace ScheduledRateUpdater.Services
{
    public class ScheduledRateUpdateBackgroundService : BackgroundService
    {
        private readonly ILogger<ScheduledRateUpdateBackgroundService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _services;
        private readonly CrontabSchedule _schedule;

        public ScheduledRateUpdateBackgroundService(
            ILogger<ScheduledRateUpdateBackgroundService> logger,
            IConfiguration configuration,
            IServiceProvider services)
        {
            _logger = logger;
            _configuration = configuration;
            _services = services;

            var cronExpression = _configuration["Scheduler:CronSchedule"];
            _schedule = CrontabSchedule.Parse(cronExpression, new CrontabSchedule.ParseOptions { IncludingSeconds = false });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var nextOccurrence = _schedule.GetNextOccurrence(DateTime.UtcNow);
                var delay = nextOccurrence - DateTime.UtcNow;

                _logger.LogInformation($"Next rate update scheduled for: {nextOccurrence} (UTC)");

                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay, stoppingToken);
                }

                // Execute the update within a new scope to ensure services are properly disposed
                using (var scope = _services.CreateScope())
                {
                    var rateUpdateService = scope.ServiceProvider.GetRequiredService<RateUpdateService>();
                    await rateUpdateService.UpdateRatesAsync();
                }
            }
        }
    }
}