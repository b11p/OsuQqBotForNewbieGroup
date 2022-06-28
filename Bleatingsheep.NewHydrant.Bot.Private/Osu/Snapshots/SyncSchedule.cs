using System;
using System.Linq;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sisters.WudiLib;

namespace Bleatingsheep.NewHydrant.Osu.Snapshots;

#nullable enable
[Component("SyncSchedule")]
public class SyncSchedule : Service, IRegularAsync
{
    private readonly IDbContextFactory<NewbieContext> _dbContextFactory;
    private readonly ILogger<SyncSchedule> _logger;

    public TimeSpan? OnUtc => new TimeSpan(19, 30, 0);

    public TimeSpan? Every => null;

    public SyncSchedule(IDbContextFactory<NewbieContext> dbContextFactory, ILogger<SyncSchedule> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public async Task RunAsync(HttpApiClient api)
    {
        await using var db1 = _dbContextFactory.CreateDbContext();
        var snapshotted =
            await db1.UserSnapshots
            .Select(s => new { s.UserId, s.Mode })
            .Distinct()
            .ToListAsync()
            .ConfigureAwait(false);
        var scheduled =
            await db1.UpdateSchedules
            .Select(s => new { s.UserId, s.Mode })
            .ToListAsync()
            .ConfigureAwait(false);
        var toSchedule = snapshotted.Except(scheduled).Select(i => new UpdateSchedule
        {
            UserId = i.UserId,
            Mode = i.Mode,
            NextUpdate = DateTimeOffset.UtcNow,
        }).ToList();
        if (toSchedule.Count > 0)
        {
            _logger.LogDebug($"Adding {toSchedule.Count} items to schedule.");
            db1.UpdateSchedules.AddRange(toSchedule);
            await db1.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
#nullable restore