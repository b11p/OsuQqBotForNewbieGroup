using Bleatingsheep.Osu;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Bleatingsheep.NewHydrant.DataMaintenance;
/// <summary>
/// Sync snapshot schedule from binded users to update schedules.
/// </summary>
public sealed class SyncScheduleService : BackgroundService
{
    private static readonly SemaphoreSlim s_semaphore = new(1);
    private readonly IDbContextFactory<NewbieContext> _dbContextFactory;
    private readonly ILogger<SyncScheduleService> _logger;

    public static TimeSpan OnUtc => new(19, 29, 59);

    public SyncScheduleService(IDbContextFactory<NewbieContext> dbContextFactory, ILogger<SyncScheduleService> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        _logger.LogInformation("Start SyncScheduleService");
        await using var db1 = _dbContextFactory.CreateDbContext();
        db1.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        var snapshotted =
            await db1.UserSnapshots
            .Select(s => new { s.UserId, s.Mode })
            .Distinct()
            .ToListAsync()
            .ConfigureAwait(false);
        var binded = await
            (from b in db1.Bindings.AsAsyncEnumerable()
             from m in new[] { Mode.Standard, Mode.Taiko, Mode.Catch, Mode.Mania }.ToAsyncEnumerable()
             select new { UserId = b.OsuId, Mode = m })
            .ToListAsync().ConfigureAwait(false);
        var scheduled =
            await db1.UpdateSchedules
            .Select(s => new { s.UserId, s.Mode })
            .ToListAsync()
            .ConfigureAwait(false);
        var toSchedule = snapshotted.Union(binded).Except(scheduled).Select(i => new UpdateSchedule
        {
            UserId = i.UserId,
            Mode = i.Mode,
            NextUpdate = DateTimeOffset.UtcNow,
        }).ToList();
        if (toSchedule.Count > 0)
        {
            _logger.LogInformation("Adding {toSchedule.Count} items to schedule.", toSchedule.Count);
            db1.UpdateSchedules.AddRange(toSchedule);
            await db1.SaveChangesAsync().ConfigureAwait(false);
        }
        _logger.LogInformation("End SyncScheduleService");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var timeToNextRun = new TimeSpan(24, 0, 0) - (DateTimeOffset.UtcNow - OnUtc).TimeOfDay;
            _logger.LogInformation("Will trigger at {OnUtc}, waiting {timeToNextRun}", OnUtc, timeToNextRun);
            await Task.Delay(timeToNextRun, stoppingToken).ConfigureAwait(false);
            try
            {
                await RunAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "error");
            }
        }
    }
}
