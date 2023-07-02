using Bleatingsheep.NewHydrant.Data;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Bleatingsheep.NewHydrant.DataMaintenance;
public class UpdateSnapshotsService : BackgroundService
{
    private readonly IDbContextFactory<NewbieContext> _dbContextFactory;
    private readonly DataMaintainer _dataMaintainer;
    private readonly ILogger<UpdateSnapshotsService> _logger;

    public UpdateSnapshotsService(IDbContextFactory<NewbieContext> dbContextFactory, DataMaintainer dataMaintainer, ILogger<UpdateSnapshotsService> logger)
    {
        _dbContextFactory = dbContextFactory;
        _dataMaintainer = dataMaintainer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromMinutes(2));
        while (await timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var db = _dbContextFactory.CreateDbContext();
                int scheduledCount = await db.UpdateSchedules.CountAsync(s => s.NextUpdate <= DateTimeOffset.UtcNow, stoppingToken);
                var toUpdate = await db.UpdateSchedules
                    .Where(s => s.NextUpdate <= DateTimeOffset.UtcNow)
                    .OrderBy(s => s.NextUpdate)
                    .Take(200)
                    .ToListAsync(stoppingToken);
                if (scheduledCount == 0)
                    continue;
                _logger.LogDebug("Updating {toUpdate.Count} of {scheduledCount} snapshots.", toUpdate.Count, scheduledCount);
                int successCount = 0;
                foreach (var schedule in toUpdate)
                {
                    try
                    {
                        await _dataMaintainer.UpdateNowAsync(schedule.UserId, schedule.Mode);
                        schedule.NextUpdate = DateTimeOffset.UtcNow + TimeSpan.FromHours(6);
                        successCount++;
                    }
                    catch (Exception e)
                    {
                        if (e.Message.Contains("429 Too Many Requests"))
                        {
                            _logger.LogInformation("Reached API rate limit on user id {schedule.UserId} mode {schedule.Mode}", schedule.UserId, schedule.Mode);
                        }
                        else
                        {
                            _logger.LogError(e, "Update error on user id {schedule.UserId} mode {schedule.Mode}", schedule.UserId, schedule.Mode);
                        }
                        break;
                    }
                }
                await db.SaveChangesAsync(stoppingToken).ConfigureAwait(false);
                _logger.LogDebug("Update schedule completed. Success {successCount} of {toUpdate.Count}", successCount, toUpdate.Count);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occured during updating snapshots");
            }
        }
    }
}
