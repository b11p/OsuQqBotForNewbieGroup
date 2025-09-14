using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core;
using Bleatingsheep.OsuMixedApi;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.Osu;

[Component("highlight")]
public sealed partial class Highlight : Service, IMessageCommand
{
    private readonly IDbContextFactory<NewbieContext> _dbContextFactory;

    public Highlight(OsuApiClient osuApi, IDbContextFactory<NewbieContext> dbContextFactory)
    {
        OsuApi = osuApi;
        _dbContextFactory = dbContextFactory;
    }

    private OsuApiClient OsuApi { get; }

    public async Task ProcessAsync(MessageContext superContext, HttpApiClient api)
    {
        var context = superContext as GroupMessage;
        var groupMembers = await api.GetGroupMemberListAsync(context.GroupId);
        Logger.Info($"群 {context.GroupId} 开启今日高光，成员共 {groupMembers.Length} 名。");

        var stopwatch = Stopwatch.StartNew();

        await using var dbContext = _dbContextFactory.CreateDbContext();
        //var bindings = await (from b in dbContext.Bindings
        //                      join mi in groupMembers on b.UserId equals mi.UserId
        //                      select new { Info = mi, Binding = b.OsuId }).ToListAsync();
        ////var history = (from bi in bindings.AsQueryable()
        ////               join ui in motherShip.Userinfo on bi.Binding equals ui.UserId into histories
        ////               select new { bi.Info, bi.Binding, History = histories.OrderByDescending(ui => ui.QueryDate).First() }).ToList();
        //var osuIds = bindings.Select(b => b.Binding).Distinct().ToList();
        var qqs = groupMembers.Select(mi => mi.UserId).ToList();
        var osuIds = await dbContext
            .Bindings.Where(bi => qqs.Contains(bi.UserId))
            .Select(bi => bi.OsuId)
            .Distinct()
            .ToListAsync();

        Logger.Info($"找到 {osuIds.Count} 个绑定信息，耗时 {stopwatch.ElapsedMilliseconds}ms。");
        if (osuIds.Count > 100)
        {
            await api.SendMessageAsync(
                context.Endpoint,
                $"开始查询今日高光，本群人数较多，预计需要 {TimeSpan.FromSeconds(osuIds.Count * 0.4).TotalMinutes:0.0} 分钟，请耐心等待。"
            );
        }

        Bleatingsheep.Osu.Mode mode = 0;
        if (!string.IsNullOrEmpty(ModeString))
        {
            try
            {
                mode = Bleatingsheep.Osu.ModeExtensions.Parse(ModeString);
            }
            catch (FormatException)
            {
                // ignore
            }
        }

        stopwatch = Stopwatch.StartNew();
        List<UserSnapshot> history = await GetHistories(osuIds, mode).ConfigureAwait(false);

        Logger.Info($"找到 {history.Count} 个历史信息，耗时 {stopwatch.ElapsedMilliseconds}ms。");
        if (history.Count == 0)
        {
            await api.SendMessageAsync(context.Endpoint, "你群根本没有人屙屎。");
            return;
        }

        var nowInfos = new ConcurrentDictionary<int, UserInfo>(-1, history.Count);
        var fails = Channel.CreateBounded<int>(history.Count);
        var (failsTx, failsRx) = (fails.Writer, fails.Reader);
        stopwatch = Stopwatch.StartNew();
        var fetchIds = history.Select(h => h.UserId).Distinct().ToList();
        int completes = 0;
        int retries = 0;
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromMinutes(10)).Token;
        await Parallel.ForEachAsync(
            fetchIds.ToAsyncEnumerable().Concat(failsRx.ReadAllAsync()),
            new ParallelOptions { MaxDegreeOfParallelism = 8 },
            async (bi, _cancellationToken) =>
            {
                var (success, userInfo) = await OsuApi
                    .GetCachedUserInfo(bi, mode)
                    .ConfigureAwait(false);
                if (!success)
                {
                    try
                    {
                        await Task.Delay(100, cancellationToken);
                    }
                    catch (TaskCanceledException)
                    {
                        // 超时，关闭 Channel，不再重试。
                        _ = failsTx.TryComplete();
                    }

                    Interlocked.Increment(ref retries);
                    _ = failsTx.TryWrite(bi);
                    // 如果写入时 Channel 已关闭，则会写入失败，这种情况下不需要再写入了。
                }
                else
                {
                    Interlocked.Increment(ref completes);
                    if (completes == fetchIds.Count)
                    {
                        _ = failsTx.TryComplete();
                    }

                    if (userInfo != null)
                    {
                        nowInfos[bi] = userInfo;
                    }
                }
            }
        );
        Logger.Info(
            $"查询 API 花费 {stopwatch.ElapsedMilliseconds}ms，失败 {fetchIds.Count - completes} 个，重试 {retries} 次。"
        );

        var errorMessages = new List<string>();
        if (cancellationToken.IsCancellationRequested)
        {
            errorMessages.Add("查询用户信息超时，部分数据可能不完整。");
        }
        if (fetchIds.Count - completes > 0)
        {
            errorMessages.Add(
                $"有 {fetchIds.Count - completes} 人增量数据查询失败，重试了 {retries} 次。"
            );
        }

        var cps = (
            from his in history
            join now in nowInfos on his.UserId equals now.Key
            where his.UserInfo.PlayCount != now.Value.PlayCount
            orderby now.Value.Performance - his.UserInfo.Performance descending
            select new
            {
                Old = his.UserInfo,
                New = now.Value,
                Meta = his,
            }
        ).ToList();

        if (cps.Count == 0)
        {
            var message = "你群根本没有人屙屎。";
            if (errorMessages.Count > 0)
            {
                var noOsuSB = new StringBuilder("你群根本没有人屙屎。\r\n\r\n错误信息：\r\n");
                noOsuSB.AppendJoin("\r\n", errorMessages);
                message = noOsuSB.ToString();
            }
            await api.SendMessageAsync(context.Endpoint, message);
            return;
        }

        var increase = cps.Find(cp =>
            cp.Old.Performance != 0 && cp.New.Performance != cp.Old.Performance
        );
        var mostPlay = cps.OrderByDescending(cp => cp.New.TotalHits - cp.Old.TotalHits).First();
        var longestPlay = cps.OrderByDescending(cp =>
                cp.New.TotalSecondsPlayed - cp.Old.TotalSecondsPlayed
            )
            .First();
        var sb = new StringBuilder(100);
        sb.AppendLine("最飞升：");
        if (increase != null)
        {
            sb.AppendLine(
                    $"{increase.New.Name} 增加了 {increase.New.Performance - increase.Old.Performance:#.##} PP。"
                )
                .AppendLine(
                    $"({increase.Old.Performance:#.##} -> {increase.New.Performance:#.##})"
                );
        }
        else
        {
            sb.AppendLine("你群没有人飞升。");
        }

        sb.AppendLine("最肝：")
            .AppendLine(
                $"{mostPlay.New.Name} 打了 {mostPlay.New.TotalHits - mostPlay.Old.TotalHits} 下。"
            );
        sb.Append(
            $"{longestPlay.New.Name} 玩儿了 {TimeSpan.FromSeconds(longestPlay.New.TotalSecondsPlayed - longestPlay.Old.TotalSecondsPlayed).TotalHours:#.##} 小时。"
        );

        if (errorMessages.Count > 0)
        {
            sb.Append("\r\n\r\n错误信息：\r\n");
            sb.AppendJoin("\r\n", errorMessages);
        }

        await api.SendMessageAsync(context.Endpoint, sb.ToString());
    }

    private async Task<List<UserSnapshot>> GetHistories(
        List<int> osuIds,
        Bleatingsheep.Osu.Mode mode
    )
    {
        await using var newbieContext = _dbContextFactory.CreateDbContext();
        var now = DateTimeOffset.UtcNow;
        var snaps = await newbieContext
            .UserSnapshots.Where(s =>
                now.Subtract(TimeSpan.FromHours(36)) < s.Date
                && s.Mode == mode
                && osuIds.Contains(s.UserId)
            )
            .ToListAsync()
            .ConfigureAwait(false);

        return snaps
            .GroupBy(s => s.UserId)
            .Select(g =>
                g.OrderBy(s => Utilities.DateUtility.GetError(now - TimeSpan.FromHours(24), s.Date))
                    .First()
            )
            .ToList();
    }

    [Parameter("mode")]
    private string ModeString { get; set; }

    [GeneratedRegex(
        @"^今日高光\s*(?:[,，]\s*(?<mode>.+?)\s*)?$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant
    )]
    private static partial Regex MatchRegex();

    public bool ShouldResponse(MessageContext context) =>
        context is GroupMessage g
        && g.Content.TryGetPlainText(out string text)
        && RegexCommand(MatchRegex(), text.Trim());
}
