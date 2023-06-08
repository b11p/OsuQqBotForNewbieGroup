using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊;
#nullable enable
[Component("RssNotification")]
public sealed class RssNotification : IMessageMonitor
{
    private const string FieldName = nameof(RssNotification) + ".state";
    private static readonly Guid s_guid = Guid.NewGuid();

    private readonly IDbContextFactory<NewbieContext> _dbContextFactory;
    private readonly IMemoryCache _memoryCache;

    public RssNotification(IDbContextFactory<NewbieContext> dbContextFactory, IMemoryCache memoryCache)
    {
        _dbContextFactory = dbContextFactory;
        _memoryCache = memoryCache;
    }

    public async Task OnMessageAsync(MessageContext message, HttpApiClient api)
    {
        if (message is not GroupMessage g)
        {
            return;
        }
        var state = await _memoryCache.GetOrCreateAsync(GetCacheKey(g.GroupId), async _ =>
        {
            await using var db = _dbContextFactory.CreateDbContext();
            using var stateInDb = await db.BotGroupFields.FirstOrDefaultAsync(f => f.GroupId == g.GroupId && f.FieldName == FieldName);
            if (stateInDb is not null)
            {
                var state = stateInDb.Data?.Deserialize<RssState>();
                if (state is not null)
                {
                    return state;
                }
                db.BotGroupFields.Remove(stateInDb);
            }
            var newState = new RssState
            {
                LastCheckDate = DateTimeOffset.UnixEpoch,
            };
            db.BotGroupFields.Add(new BotGroupField
            {
                GroupId = g.GroupId,
                FieldName = FieldName,
                Data = JsonSerializer.SerializeToDocument(newState),
            });
            await db.SaveChangesAsync();
            return newState;
        });


    }

    private static (long, Guid) GetCacheKey(long groupId)
    {
        return (groupId, s_guid);
    }

    private sealed class RssState
    {
        public DateTimeOffset LastCheckDate { get; set; }
    }

    private sealed class RssFeedList
    {
        public required List<RssFeedInfo> Feeds { get; set; }
    }

    private sealed class RssFeedInfo
    {
        public required string Url { get; init; }
        public required
    }
}