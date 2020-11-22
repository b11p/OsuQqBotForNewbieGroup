using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.OsuMixedApi;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using MotherShipDatabase;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.Osu
{
    [Component("highlight")]
    public sealed class Highlight : OsuFunction, IMessageCommand
    {
        public async Task ProcessAsync(MessageContext superContext, HttpApiClient api)
        {
            var context = superContext as GroupMessage;
            var groupMembers = await api.GetGroupMemberListAsync(context.GroupId);
            Logger.Debug($"群 {context.GroupId} 开启今日高光，成员共 {groupMembers.Length} 名。");

            var stopwatch = Stopwatch.StartNew();

            using (var dbContext = new NewbieContext())
            {
                //var bindings = await (from b in dbContext.Bindings
                //                      join mi in groupMembers on b.UserId equals mi.UserId
                //                      select new { Info = mi, Binding = b.OsuId }).ToListAsync();
                ////var history = (from bi in bindings.AsQueryable()
                ////               join ui in motherShip.Userinfo on bi.Binding equals ui.UserId into histories
                ////               select new { bi.Info, bi.Binding, History = histories.OrderByDescending(ui => ui.QueryDate).First() }).ToList();
                //var osuIds = bindings.Select(b => b.Binding).Distinct().ToList();
                var qqs = groupMembers.Select(mi => mi.UserId).ToList();
                var osuIds = await dbContext.Bindings.Where(bi => qqs.Contains(bi.UserId)).Select(bi => bi.OsuId).Distinct().ToListAsync();

                Logger.Debug($"找到 {osuIds.Count} 个绑定信息，耗时 {stopwatch.ElapsedMilliseconds}ms。");

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

                Logger.Debug($"找到 {history.Count} 个历史信息，耗时 {stopwatch.ElapsedMilliseconds}ms。");

                // Using ConcurrentBag is enough here. ConcurrentDictionary is unnecessary and costly.
                var nowInfos = new ConcurrentDictionary<int, UserInfo>(10, history.Count);
                var fails = new BlockingCollection<int>();
                stopwatch = Stopwatch.StartNew();
                var fetchIds = history.Select(h => (int)h.UserId).Distinct().ToList();
                int completes = 0;
                var cancellationToken = new CancellationTokenSource(TimeSpan.FromMinutes(1)).Token;
                var tasks = fetchIds.Concat(fails.GetConsumingEnumerable()).Select(async bi =>
                {
                    var (success, userInfo) = await GetCachedUserInfo(bi, (Bleatingsheep.Osu.Mode)mode).ConfigureAwait(false);
                    if (!success)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            fails.CompleteAdding();
                        if (!fails.IsAddingCompleted)
                            try
                            {
                                fails.Add(bi);
                            }
                            catch (InvalidOperationException)
                            {
                            }
                    }
                    else
                    {
                        Interlocked.Increment(ref completes);
                        if (completes == fetchIds.Count)
                            fails.CompleteAdding();
                        if (userInfo != null)
                            nowInfos[bi] = userInfo;
                    }
                }).ToArray();
                await Task.WhenAll(tasks).ConfigureAwait(false);
                Logger.Debug($"查询 API 花费 {stopwatch.ElapsedMilliseconds}ms，失败 {fails.Count} 个。");

                var cps = (from his in history
                           join now in nowInfos on his.UserId equals now.Key
                           where his.UserInfo.PlayCount != now.Value.PlayCount
                           orderby now.Value.Performance - his.UserInfo.Performance descending
                           select new { Old = his.UserInfo, New = now.Value, Meta = his }).ToList();

                if (fails.Count > 0)
                {
                    await api.SendGroupMessageAsync(context.GroupId, $"失败了 {fails.Count} 人。");
                }
                if (cps.Count == 0)
                {
                    await api.SendMessageAsync(context.Endpoint, "你群根本没有人屙屎。");
                    return;
                }
                else
                {
                    var increase = cps.Find(cp => cp.Old.Performance != 0 && cp.New.Performance != cp.Old.Performance);
                    var mostPlay = cps.OrderByDescending(cp => cp.New.TotalHits - cp.Old.TotalHits).First();
                    var sb = new StringBuilder(100);
                    sb.AppendLine("最飞升：");
                    if (increase != null)
                        // sb.AppendLine($"{increase.New.Name} 增加了 {increase.New.Performance - increase.Old.Performance:#.##} PP。")
                        sb.Append(increase.New.Name).Append(" 增加了 ").AppendFormat("{0:#.##}", increase.New.Performance - increase.Old.Performance).AppendLine(" PP。")
                            // .AppendLine($"({increase.Old.Performance:#.##} -> {increase.New.Performance:#.##})");
                            .Append('(').AppendFormat("{0:#.##}", increase.Old.Performance).Append(" -> ").AppendFormat("{0:#.##}", increase.New.Performance).AppendLine(")");
                    else
                        sb.AppendLine("你群没有人飞升。");
                    sb.AppendLine("最肝：")
                        // .Append($"{mostPlay.New.Name} 打了 {mostPlay.New.TotalHits - mostPlay.Old.TotalHits} 下。");
                        .Append(mostPlay.New.Name).Append(" 打了 ").Append(mostPlay.New.TotalHits - mostPlay.Old.TotalHits).Append(" 下。");


                    await api.SendMessageAsync(context.Endpoint, sb.ToString());
                }
            }
        }

        private static async Task<List<UserSnapshot>> GetHistories(List<int> osuIds, Bleatingsheep.Osu.Mode mode)
        {
            using var newbieContext = new NewbieContext();
            var now = DateTimeOffset.Now;
            var snaps = await newbieContext.UserSnapshots
                .Where(s => now.Subtract(TimeSpan.FromHours(36)) < s.Date && s.Mode == mode && osuIds.Contains(s.UserId))
                .ToListAsync().ConfigureAwait(false);

            return snaps
                .GroupBy(s => s.UserId)
                .Select(g => g.OrderBy(s => Utilities.DateUtility.GetError(now - TimeSpan.FromHours(24), s.Date)).First())
                .ToList();
        }

        private sealed class UserInfoComparier : IEqualityComparer<Userinfo>
        {
            public bool Equals(Userinfo x, Userinfo y)
                => (x.UserId, x.Mode) == (y.UserId, y.Mode);

            public int GetHashCode(Userinfo obj)
                => ((int)obj.UserId << 2) + (int)obj.Mode;
        }

        [Parameter("mode")]
        private string ModeString { get; set; }

        private static readonly Regex s_regex = new Regex(@"^今日高光\s*(?:[,，]\s*(?<mode>.+?)\s*)?$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public bool ShouldResponse(MessageContext context)
            => context is GroupMessage g
                && g.Content.TryGetPlainText(out string text)
                && RegexCommand(s_regex, text.Trim());
    }
}
