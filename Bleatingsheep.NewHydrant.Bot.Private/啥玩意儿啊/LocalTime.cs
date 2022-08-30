using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊;
#nullable enable
[Component("time")]
public class LocalTime : IMessageCommand
{
    private static readonly ConcurrentDictionary<(long, long), Channel<GroupMessage>> s_channels = new();
    private readonly IDbContextFactory<NewbieContext> _dbContextFactory;
    private int _errTime = 0;

    public LocalTime(IDbContextFactory<NewbieContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task ProcessAsync(MessageContext context, HttpApiClient api)
    {
        var g = (GroupMessage)context;
        if (s_channels.TryGetValue((g.GroupId, g.UserId), out var chan))
        {
            await chan.Writer.WriteAsync(g).ConfigureAwait(false);
        }
        else if (g.Content.TryGetPlainText(out var text) && text.Trim() == "添加时区")
        {
            Channel<GroupMessage> newChan = Channel.CreateUnbounded<GroupMessage>();
            if (s_channels.TryAdd((g.GroupId, g.UserId), newChan))
            {
                var groupId = g.GroupId;
                var userId = g.UserId;
                _ = Task.Run(async () =>
                {
                    string? next = nameof(GetTimeZoneAsync);
                    try
                    {
                        await api.SendGroupMessageAsync(groupId, "请输入要添加的时区（例如：Asia/Shanghai）").ConfigureAwait(false);
                        var chanReader = newChan.Reader;
                        while (await chanReader.WaitToReadAsync(new CancellationTokenSource(TimeSpan.FromMinutes(1)).Token).ConfigureAwait(false))
                        {
                            if (!chanReader.TryRead(out var msg))
                                continue;
                            switch (next)
                            {
                                case nameof(GetTimeZoneAsync):
                                    next = await GetTimeZoneAsync(msg, api).ConfigureAwait(false);
                                    break;
                                default:
                                    await api.SendGroupMessageAsync(groupId, "执行错误。").ConfigureAwait(false);
                                    next = null;
                                    break;
                            }

                            if (next is null)
                            {
                                return;
                            }
                        }
                    }
                    finally
                    {
                        s_channels.TryRemove((groupId, userId), out _);
                        newChan.Writer.Complete();
                    }
                });
            }
        }
    }

    public bool ShouldResponse(MessageContext context)
    {
        if (context is not GroupMessage g)
        {
            return false;
        }
        if (s_channels.TryGetValue((g.GroupId, g.UserId), out _))
        {
            return true;
        }
        else if (g.Content.TryGetPlainText(out var text) && text.Trim() == "添加时区")
        {
            return true;
        }
        return false;
    }

    public async ValueTask<string?> GetTimeZoneAsync(GroupMessage g, HttpApiClient api)
    {
        if (g.Content.TryGetPlainText(out var text))
        {
            text = text.Trim();
            if ("T".Equals(text, StringComparison.OrdinalIgnoreCase))
            {
                await api.SendGroupMessageAsync(g.GroupId, "已退出。").ConfigureAwait(false);
                return null;
            }
            TimeZoneInfo tz;
            try
            {
                tz = TimeZoneInfo.FindSystemTimeZoneById(text);
                if (!tz.HasIanaId)
                {
                    await api.SendGroupMessageAsync(g.GroupId, "时区格式不正确，请确保使用 IANA ID，请重新发送。回T退出。").ConfigureAwait(false);
                    return nameof(GetTimeZoneAsync);
                }
            }
            catch (TimeZoneNotFoundException)
            {
                _errTime++;
                if (_errTime < 3)
                {
                    await api.SendGroupMessageAsync(g.GroupId, $"未找到时区，请重新发送。回T退出。").ConfigureAwait(false);
                    return nameof(GetTimeZoneAsync);
                }
                else
                {
                    await api.SendGroupMessageAsync(g.GroupId, "未找到时区，本次服务已结束。").ConfigureAwait(false);
                    return null;
                }
            }
            catch (Exception)
            {
                await api.SendGroupMessageAsync(g.GroupId, "查找时区失败。本次服务已结束。").ConfigureAwait(false);
                return null;
            }

            var displayName = tz.DisplayName;
            var tzId = tz.Id;

            await using var db = _dbContextFactory.CreateDbContext();
            var field = await db.BotGroupFields.FirstOrDefaultAsync(f => f.GroupId == g.GroupId && f.FieldName == "member_timezones").ConfigureAwait(false);
            GroupTimeZoneList? list = field?.Data?.Deserialize<GroupTimeZoneList>();
            if (field is null)
            {
                field = new()
                {
                    GroupId = g.GroupId,
                    FieldName = "member_timezones",
                };
                db.BotGroupFields.Add(field);
            }

            // check if the time zone already exists
            if (list?.TimeZones.Any(z => z.DisplayName == displayName) == true)
            {
                await api.SendGroupMessageAsync(g.GroupId, $"时区{displayName}已存在。本次服务已结束。").ConfigureAwait(false);
                return null;
            }

            // update
            if (list is null)
            {
                list = new()
                {
                    TimeZones = new(),
                };
            }
            list.TimeZones.Add(new() { TimeZoneId = tzId, DisplayName = displayName });
            field.Data = JsonSerializer.SerializeToDocument(list);
            await db.SaveChangesAsync().ConfigureAwait(false);
            await api.SendGroupMessageAsync(g.GroupId, "已添加时区，发送“时差”查看。本次服务已结束。").ConfigureAwait(false);
            return null;
        }
        else
            // 非纯文本消息直接忽略
            return nameof(GetTimeZoneAsync);
    }
}

sealed class GroupTimeZoneList
{
    public List<GroupTimeZone> TimeZones { get; set; } = default!;
}

sealed class GroupTimeZone
{
    public string TimeZoneId { get; set; } = default!;
    public string DisplayName { get; set; } = default!;
}
#nullable restore