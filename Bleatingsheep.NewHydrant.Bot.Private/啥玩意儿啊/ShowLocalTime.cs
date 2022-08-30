using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊;
#nullable enable
[Component("show_local_time")]
public class ShowLocalTime : IMessageCommand
{
    private readonly IDbContextFactory<NewbieContext> _dbContextFactory;

    public ShowLocalTime(IDbContextFactory<NewbieContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task ProcessAsync(MessageContext context, HttpApiClient api)
    {
        var g = (GroupMessage)context;
        await using var db = _dbContextFactory.CreateDbContext();
        var field = await db.BotGroupFields.FirstOrDefaultAsync(f => f.GroupId == g.GroupId && f.FieldName == "member_timezones").ConfigureAwait(false);
        var tzList = field?.Data?.Deserialize<GroupTimeZoneList>();
        if (tzList?.TimeZones.Count is not > 0)
        {
            await api.SendGroupMessageAsync(g.GroupId, "还没有设置群友时区呢，发送“添加时区”。").ConfigureAwait(false);
            return;
        }
        var now = DateTime.UtcNow;
        var resultList = tzList.TimeZones.Select(tz =>
        {
            var tzi = TimeZoneInfo.FindSystemTimeZoneById(tz.TimeZoneId);
            return $"{tz.DisplayName}: {TimeZoneInfo.ConvertTimeFromUtc(now, tzi):d, dddd H:mm}";
        });
        var result = string.Join("\r\n", resultList);
        await api.SendGroupMessageAsync(g.GroupId, result).ConfigureAwait(false);
    }

    public bool ShouldResponse(MessageContext context)
    {
        return context is GroupMessage g && g.Content.TryGetPlainText(out var text) && text.Trim() == "时差";
    }
}
#nullable restore