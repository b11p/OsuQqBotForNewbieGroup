using Bleatingsheep.NewHydrant.Data;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Bleatingsheep.NewHydrant.DataMaintenance;

public class UpdateSnapshotsService : BackgroundService
{
    private static readonly TimeSpan s_updateScheduleDefault = TimeSpan.FromHours(8);
    private static readonly TimeSpan s_updateScheduleActive = TimeSpan.FromHours(2); // when active within the interval
    private static readonly TimeSpan s_updateScheduleSemiActive = TimeSpan.FromHours(4); // when API returns some recent play
    private static readonly TimeSpan s_updateScheduleInactive = TimeSpan.FromDays(2); // when banned or inactive
    private static readonly TimeSpan s_updateScheduleNotAdded = TimeSpan.FromHours(6); // when not added snapshots (due to completely same profile with most recent snapshot)

    private readonly IDbContextFactory<NewbieContext> _dbContextFactory;
    private readonly DataMaintainer _dataMaintainer;
    private readonly ILogger<UpdateSnapshotsService> _logger;

    public UpdateSnapshotsService(
        IDbContextFactory<NewbieContext> dbContextFactory,
        DataMaintainer dataMaintainer,
        ILogger<UpdateSnapshotsService> logger
    )
    {
        _dbContextFactory = dbContextFactory;
        _dataMaintainer = dataMaintainer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        while (
            await timer.WaitForNextTickAsync(stoppingToken)
            && !stoppingToken.IsCancellationRequested
        )
        {
            try
            {
                await using var db = _dbContextFactory.CreateDbContext();
                int scheduledCount = await db.UpdateSchedules.CountAsync(
                    s => s.NextUpdate <= DateTimeOffset.UtcNow,
                    stoppingToken
                );
                if (scheduledCount == 0)
                {
                    continue;
                }

                if (scheduledCount >= 500)
                {
                    _logger.LogWarning("Backlog is {count}", scheduledCount);
                }
                var toUpdate = await db
                    .UpdateSchedules.Where(s => s.NextUpdate <= DateTimeOffset.UtcNow)
                    .OrderBy(s => s.NextUpdate)
                    .Take(160)
                    .ToListAsync(stoppingToken);
                _logger.LogDebug(
                    "Updating {toUpdate.Count} of {scheduledCount} snapshots.",
                    toUpdate.Count,
                    scheduledCount
                );
                int successCount = 0;
                int normalCount = 0;
                int activeCount = 0;
                int semiActiveCount = 0;
                int inactiveCount = 0;
                int noChangeCount = 0;
                foreach (var schedule in toUpdate)
                {
                    try
                    {
                        var report = await _dataMaintainer.UpdateNowAsync(
                            schedule.UserId,
                            schedule.Mode
                        );
                        TimeSpan nextDelay;
                        if (report.UserNotExists || report.Inactive)
                        {
                            nextDelay = s_updateScheduleInactive;
                            inactiveCount++;
                        }
                        else if (report.AddedPlayRecords.Count > 0)
                        {
                            nextDelay = s_updateScheduleActive;
                            activeCount++;
                        }
                        else if (report.UserRecents.Length > 0)
                        {
                            nextDelay = s_updateScheduleSemiActive;
                            semiActiveCount++;
                        }
                        else if (!report.AddedSnapshot)
                        {
                            nextDelay = s_updateScheduleNotAdded;
                            noChangeCount++;
                        }
                        else
                        {
                            nextDelay = s_updateScheduleDefault;
                            normalCount++;
                        }
                        schedule.NextUpdate = DateTimeOffset.UtcNow + nextDelay;
                        successCount++;
                    }
                    catch (Exception e)
                    {
                        if (e.Message.Contains("429 Too Many Requests"))
                        {
                            _logger.LogInformation(
                                "Reached API rate limit on user id {schedule.UserId} mode {schedule.Mode}",
                                schedule.UserId,
                                schedule.Mode
                            );
                        }
                        else if (e.Message.Contains("502 Bad Gateway"))
                        {
                            _logger.LogInformation(
                                "Server error, breaking to wait for server to get normal. user id {schedule.UserId} mode {schedule.Mode}",
                                schedule.UserId,
                                schedule.Mode
                            );
                        }
                        else
                        {
                            _logger.LogError(
                                e,
                                "Update error on user id {schedule.UserId} mode {schedule.Mode}",
                                schedule.UserId,
                                schedule.Mode
                            );
                        }
                        break;
                    }
                }
                await db.SaveChangesAsync(stoppingToken).ConfigureAwait(false); // 将 schedule 更新持久化到数据库
                _logger.LogDebug(
                    "Update schedule completed. Success {successCount} of {toUpdate.Count}.",
                    successCount,
                    toUpdate.Count
                );
                _logger.LogDebug(
                    "Normal: {normalCount}, Active: {activeCount}, SemiActive: {semiActiveCount}, Inactive: {inactiveCount}, No Change: {noChangeCount}",
                    normalCount,
                    activeCount,
                    semiActiveCount,
                    inactiveCount,
                    noChangeCount
                );
            }
            catch (DbUpdateConcurrencyException e)
            {
                _logger.LogWarning(
                    "数据库更新并发冲突，可能是其他服务修改了更新 schedule：{message}。",
                    e.Message
                );
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occured during updating snapshots");
            }
        }
    }
}
