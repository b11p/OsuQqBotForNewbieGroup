using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.OsuMixedApi;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.Osu
{
    [Function("highlight")]
    public sealed class Highlight : OsuFunction, IMessageCommand
    {
        public async Task ProcessAsync(MessageContext superContext, HttpApiClient api)
        {
            var context = superContext as GroupMessage;
            var groupMembers = await api.GetGroupMemberListAsync(context.GroupId);
            Logger.Debug($"群 {context.GroupId} 开启今日高光，成员共 {groupMembers.Length} 名。");

            var stopwatch = Stopwatch.StartNew();

            using (var dbContext = new NewbieContext())
            using (var motherShip = new 秘密功能.MotherShipDatabase.OsuContext())
            {
                var bindings = await (from b in dbContext.Bindings
                                      join mi in groupMembers on b.UserId equals mi.UserId
                                      select new { Info = mi, Binding = b.OsuId }).ToListAsync();
                //var history = (from bi in bindings.AsQueryable()
                //               join ui in motherShip.Userinfo on bi.Binding equals ui.UserId into histories
                //               select new { bi.Info, bi.Binding, History = histories.OrderByDescending(ui => ui.QueryDate).First() }).ToList();
                var osuIds = bindings.Select(b => b.Binding).Distinct().ToList();
                int mode = 0;

                // old query
                var history = await
                    (from mother in motherShip.Userinfo.FromSql(
                        @"SELECT a.*
FROM (SELECT user_id, `mode`, max(queryDate) queryDate
FROM userinfo
WHERE `mode`={0}
GROUP BY user_id
) b JOIN userinfo a ON a.user_id = b.user_id AND a.queryDate = b.queryDate AND a.`mode` = b.`mode`", mode)
                     join bi in osuIds on mother.UserId equals bi
                     select mother).ToListAsync();

                Logger.Debug($"找到 {history.Count} 个绑定及历史信息，耗时 {stopwatch.ElapsedMilliseconds}ms。");

                var nowInfos = new ConcurrentDictionary<int, UserInfo>(10, history.Count);
                var fails = new ConcurrentBag<int>();
                stopwatch = Stopwatch.StartNew();
                Parallel.ForEach(history.Select(h => (int)h.UserId).Distinct(), bi =>
                {
                    var (success, userInfo) = OsuApi.GetUserInfoAsync(bi, (Mode)mode).GetAwaiter().GetResult();
                    if (!success)
                        fails.Add(bi);
                    else if (userInfo != null)
                        nowInfos[bi] = userInfo;
                });
                Logger.Debug($"查询 API 花费 {stopwatch.ElapsedMilliseconds}ms，失败 {fails.Count} 个。");

                var cps = (from his in history
                           join now in nowInfos on his.UserId equals now.Key
                           where his.Playcount != now.Value.PlayCount
                           orderby now.Value.Performance - (double?)his.PpRaw ?? double.PositiveInfinity descending
                           select new { Old = his, New = now.Value }).ToList();

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
                    var increase = cps.Where(cp => cp.Old.PpRaw != 0 && cp.New.Performance != (double?)cp.Old.PpRaw).FirstOrDefault();
                    var mostPlay = cps.OrderByDescending(cp => cp.New.Count300 + cp.New.Count100 + cp.New.Count50 - cp.Old.Count300 - cp.Old.Count100 - cp.Old.Count50).First();
                    var sb = new StringBuilder(100);
                    sb.AppendLine("最飞升：");
                    if (increase != null)
                        sb.AppendLine($"{increase.New.Name} 增加了 {increase.New.Performance - (double)increase.Old.PpRaw:#.##} PP。")
                            .AppendLine($"({increase.Old.PpRaw:#.##} -> {increase.New.Performance:#.##})");
                    else
                        sb.AppendLine("你群没有人飞升。");
                    sb.AppendLine("最肝：")
                        .Append($"{mostPlay.New.Name} 打了 {mostPlay.New.Count300 + mostPlay.New.Count100 + mostPlay.New.Count50 - mostPlay.Old.Count300 - mostPlay.Old.Count100 - mostPlay.Old.Count50} 下。");


                    await api.SendMessageAsync(context.Endpoint, sb.ToString());
                }
            }
        }

        public bool ShouldResponse(MessageContext context)
            => context is GroupMessage g
                && g.Content.TryGetPlainText(out string text)
                && text == "今日高光";
    }
}
