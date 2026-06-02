using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EconomiaComHistoria.Infrastructure.Services;

public class WeeklyRankingJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WeeklyRankingJob> _logger;

    public WeeklyRankingJob(IServiceProvider serviceProvider, ILogger<WeeklyRankingJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            // Sunday 23:59
            if (now.DayOfWeek == DayOfWeek.Sunday && now.Hour == 23 && now.Minute == 59)
            {
                _logger.LogInformation("Generating weekly ranking snapshot...");
                using var scope = _serviceProvider.CreateScope();
                var rankingService = scope.ServiceProvider.GetRequiredService<IRankingService>();
                await rankingService.GerarSnapshotSemanalAsync();

                // Wait for a minute to avoid double-triggering in the same minute
                await Task.Delay(60000, stoppingToken);
            }

            await Task.Delay(60000, stoppingToken); // Check every minute
        }
    }
}
