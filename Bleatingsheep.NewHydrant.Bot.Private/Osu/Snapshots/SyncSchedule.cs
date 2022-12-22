using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core;
using Bleatingsheep.NewHydrant.Data;
using Bleatingsheep.Osu;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sisters.WudiLib;

namespace Bleatingsheep.NewHydrant.Osu.Snapshots;

#nullable enable
[Component("SyncSchedule")]
public class SyncSchedule : Service, IRegularAsync
{
    private static readonly SemaphoreSlim s_semaphore = new(1);
    private readonly IDbContextFactory<NewbieContext> _dbContextFactory;
    private readonly ILogger<SyncSchedule> _logger;
    private readonly IDataProvider _dataProvider;

    public TimeSpan? OnUtc => new TimeSpan(19, 30, 0);

    public TimeSpan? Every => null;

    public SyncSchedule(IDbContextFactory<NewbieContext> dbContextFactory, ILogger<SyncSchedule> logger, IDataProvider dataProvider)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
        _dataProvider = dataProvider;
    }

    public async Task RunAsync(HttpApiClient api)
    {
        if (!s_semaphore.Wait(0))
        {
            return;
        }
        try
        {
            await using var db1 = _dbContextFactory.CreateDbContext();
            var snapshotted =
                await db1.UserSnapshots
                .Select(s => new { s.UserId, s.Mode })
                .Distinct()
                .ToListAsync()
                .ConfigureAwait(false);
            var binded = await
                (from b in db1.Bindings
                 from m in new[] { Mode.Standard, Mode.Taiko, Mode.Catch, Mode.Mania }
                 select new { UserId = b.OsuId, Mode = m })
                .ToListAsync().ConfigureAwait(false);
            var scheduled =
                await db1.UpdateSchedules
                .Select(s => new { s.UserId, s.Mode })
                .ToListAsync()
                .ConfigureAwait(false);
            var toSchedule = snapshotted.Intersect(binded).Except(scheduled).Select(i => new UpdateSchedule
            {
                UserId = i.UserId,
                Mode = i.Mode,
                NextUpdate = DateTimeOffset.UtcNow,
            }).ToList();
            if (toSchedule.Count > 0)
            {
                _logger.LogDebug("Adding {toSchedule.Count} items to schedule.", toSchedule.Count);
                db1.UpdateSchedules.AddRange(toSchedule);
                await db1.SaveChangesAsync().ConfigureAwait(false);
            }

            // cache beatmap information
            var played = await db1.UserPlayRecords.AsNoTracking().Select(r => new { r.Record.BeatmapId, r.Mode }).Distinct().ToListAsync().ConfigureAwait(false);
            var cached = await db1.BeatmapInfoCache.AsNoTracking().Select(c => new { c.BeatmapId, c.Mode }).Distinct().ToListAsync().ConfigureAwait(false);
            var noCache = played.Except(cached).ToList();
            _logger.LogInformation("Need {noCacheBid.Count} new cache.", noCache.Count);
            foreach (var beatmap in noCache)
            {
                _ = await _dataProvider.GetBeatmapInfoAsync(beatmap.BeatmapId, beatmap.Mode).ConfigureAwait(false);
            }
            _logger.LogInformation("Caching complete.");
        }
        finally
        {
            s_semaphore.Release();
        }
    }
}
#nullable restore