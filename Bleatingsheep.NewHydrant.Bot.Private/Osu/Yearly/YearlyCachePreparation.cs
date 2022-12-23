using System;
using System.Linq;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Data;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sisters.WudiLib;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.Osu.Yearly;
#nullable enable
[Component("YearlyCachePreparation")]
public class YearlyCachePreparation : IMessageCommand
{
    private readonly IDbContextFactory<NewbieContext> _dbContextFactory;
    private readonly ILogger<YearlyCachePreparation> _logger;
    private readonly IDataProvider _dataProvider;

    public YearlyCachePreparation(IDbContextFactory<NewbieContext> dbContextFactory, IDataProvider dataProvider, ILogger<YearlyCachePreparation> logger)
    {
        _dbContextFactory = dbContextFactory;
        _dataProvider = dataProvider;
        _logger = logger;
    }

    private async Task CacheBeatmapInfo()
    {
        await using var db1 = _dbContextFactory.CreateDbContext();
        // cache beatmap information
        var played = await db1.UserPlayRecords.AsNoTracking().Select(r => new { r.Record.BeatmapId, r.Mode }).Distinct().ToListAsync().ConfigureAwait(false);
        var cached = await db1.BeatmapInfoCache.AsNoTracking().Select(c => new { c.BeatmapId, c.Mode }).Distinct().AsAsyncEnumerable().ToHashSetAsync().ConfigureAwait(false);
        var random = new Random();
        var noCache = played.Except(cached).OrderBy(_ => random.Next()).ToList();
        _logger.LogInformation("Need {noCacheBid.Count} new cache.", noCache.Count);
        var success = 0;
        var failed = 0;
        foreach (var beatmap in noCache)
        {
            try
            {
                _ = await _dataProvider.GetBeatmapInfoAsync(beatmap.BeatmapId, beatmap.Mode).ConfigureAwait(false);
                success++;
            }
            catch (Exception e)
            {
                if (failed == 0)
                {
                    _logger.LogError(e, "error");
                }
                // ignore
                failed++;
            }
        }
        _logger.LogInformation("Caching complete. success {success}, failed {failed}", success, failed);
    }

    public bool ShouldResponse(MessageContext context)
    {
        return context.UserId == 962549599
            && context.Content.TryGetPlainText(out var text)
            && text == "缓存测试";
    }

    public async Task ProcessAsync(MessageContext context, HttpApiClient api)
    {
        await CacheBeatmapInfo().ConfigureAwait(false);
    }
}