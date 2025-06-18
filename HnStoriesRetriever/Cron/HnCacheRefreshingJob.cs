namespace HnStoriesRetriever.Cron;

using System;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class HnCacheRefreshingJob(ILogger<HnCacheRefreshingJob> logger, IHnService hnService) : BackgroundService
{
  private readonly CronExpression _cron = CronExpression.Parse("* * * * *");
  private readonly TimeZoneInfo _timeZone = TimeZoneInfo.Local;

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      var next = _cron.GetNextOccurrence(DateTimeOffset.Now, _timeZone, true);
      if (next.HasValue)
      {
        var delay = next.Value - DateTimeOffset.Now;
        if (delay > TimeSpan.Zero)
          await Task.Delay(delay, stoppingToken);
      }

      if (!stoppingToken.IsCancellationRequested)
      {
        logger.LogInformation("Cron job started at: {Time}", DateTimeOffset.Now);
        await hnService.RefreshList(stoppingToken);
        logger.LogInformation("Cron job finished at: {Time}", DateTimeOffset.Now);
      }
    }
  }
}
