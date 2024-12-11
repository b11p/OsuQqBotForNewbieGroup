using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core;
using Bleatingsheep.NewHydrant.Data;
using Bleatingsheep.Osu;
using Bleatingsheep.Osu.ApiClient;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.Osu.Newbie
{
#nullable enable
    [Component("newbie_statistics")]
    public class 统计新人群成员 : Service, IMessageCommand
    {
        private const int 管理群号 = 695600319;
        private const string DestPath = "/outputs/statistics_{0}.csv";
        private const string ResourceUrl = "https://res.bleatingsheep.org/statistics_{0}.csv";

        private static readonly Dictionary<string, (long, int, int)> s_groups = new()
        {
            { "旧新人群", (595985887, 3000, 190) },
            { "新人群", (231094840, 3000, 190) },
            { "进阶群", (928936255, 4500, 280) },
            { "高阶群", (281624271, 6000, 360) },
        };

        private static readonly Regex s_regex = new Regex("^统计(?<group>.+?)玩家$", RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private readonly IDbContextFactory<NewbieContext> _contextFactory;
        private readonly Lazy<IDataProvider> _dataProvider;
        private readonly ILogger<统计新人群成员> _logger;

        public 统计新人群成员(IDbContextFactory<NewbieContext> newbieContext, Lazy<IDataProvider> dataProvider, ILogger<统计新人群成员> logger)
        {
            _contextFactory = newbieContext;
            _dataProvider = dataProvider;
            _logger = logger;
        }

        [Parameter("group")]
        public string ProcessingGroupName { get; set; } = default!;

        public Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            if (s_groups.TryGetValue(ProcessingGroupName, out var t))
            {
                var (groupId, limitSum, limitBp1) = t;
                return AnalyzeGroupMember(context, api, limitSum, limitBp1, groupId);
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        private async Task AnalyzeGroupMember(MessageContext context, HttpApiClient api, int limitSum, int limitBp1, long groupId)
        {
            await api.SendMessageAsync(context.Endpoint, $"即将统计新人群玩家列表，PP 超过 {limitSum} 将标记为超限。");

            await using var dbContext = _contextFactory.CreateDbContext();
            dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            var infoList = await api.GetGroupMemberListAsync(groupId);
            Logger.Info($"Find {infoList.Length} users.");
            var userIdList = infoList.Select(i => i.UserId).ToList();
            IQueryable<BindingInfo> bindingQuery = dbContext.Bindings.Where(b => userIdList.Contains(b.UserId));
            var bindingList = await bindingQuery.ToListAsync().ConfigureAwait(false);
            Logger.Info($"Find {bindingList.Count} binding info.");

            var osuIds = bindingList.Select(b => b.OsuId).Distinct().ToList();
            var snapshotNoEarlierThan = DateTime.UtcNow.AddDays(-1);
            var snapshots = await dbContext.UserSnapshots
                .Where(s => s.Date > snapshotNoEarlierThan && osuIds.Contains(s.UserId) && s.Mode == Mode.Standard)
                .AsAsyncEnumerable()
                .GroupBy(s => s.UserId)
                .ToDictionaryAwaitAsync(g => ValueTask.FromResult(g.Key), g => g.OrderByDescending(s => s.Date).FirstOrDefaultAsync())
                .ConfigureAwait(false);
            Logger.Info($"Find {snapshots.Count} snapshots.");

            // 
            Task<(BindingInfo BindingInfo, bool Successful, UserInfo? UserInfo, UserBest? bp1)>[] userInfoTaskArray = bindingList.Select(async b =>
            {
                var o = b.OsuId;
                UserInfo? userInfo = default;
                try
                {
                    var snapshotInfo = snapshots.GetValueOrDefault(o);
                    userInfo = snapshotInfo == null
                        ? (UserInfo?)await _dataProvider.Value.GetUserInfoRetryAsync(o, Mode.Standard)
                        : snapshotInfo.UserInfo;
                    var bp1 = await _dataProvider.Value.GetUserBestLimitRetryAsync(o, Mode.Standard, 1);
                    return (BindingInfo: b, Successful: true, UserInfo: userInfo, bp1?.FirstOrDefault());
                }
                catch (Exception)
                {
                    // 即使出现异常，userInfo也可能是可用的。
                    return (b, false, userInfo, null);
                }
            })
                .ToArray();
            await Task.WhenAll(userInfoTaskArray).ConfigureAwait(false);
            Logger.Info("Complete fetching user info.");
            var userInfo = userInfoTaskArray.Select(t => t.Result);
            var results = from i in infoList
                          join u in userInfo
                              on i.UserId equals u.BindingInfo.UserId into validUsers
                          from v in validUsers.DefaultIfEmpty()
                          let b = v.BindingInfo
                          let netOk = v.Successful
                          let user = v.UserInfo
                          let bp1 = v.bp1?.Performance
                          select new
                          {
                              qq = i.UserId,
                              uid = b?.OsuId,
                              name = user?.Name,
                              pp = user?.Performance,
                              card = i.DisplayName,
                              bp1pp = bp1,
                              remark = GetTipMessage(b, netOk, user, limitSum, bp1, limitBp1)
                          };
            var writer = new StringWriter();
            using (var csvWriter = new CsvHelper.CsvWriter(writer, CultureInfo.InvariantCulture))
                await csvWriter.WriteRecordsAsync(results).ConfigureAwait(false);
            var message = writer.ToString();
            Logger.Info(message);
            await File.WriteAllTextAsync(string.Format(DestPath, groupId), message, new System.Text.UTF8Encoding(true));
            await api.SendMessageAsync(context.Endpoint, $"统计完成，前往 {string.Format(ResourceUrl, groupId)} 查看结果。");

            // add users not having snapshots to schedule
            // get scheduled users
            var scheduled = await dbContext.UpdateSchedules
                .Where(s => s.Mode == Mode.Standard && osuIds.Contains(s.UserId))
                .Select(s => s.UserId)
                .ToListAsync().ConfigureAwait(false);
            // get users not scheduled
            var notScheduled = osuIds.Except(scheduled).ToList();
            Logger.Info($"{notScheduled.Count} users not scheduled to create snapshots.");
            // add users not scheduled
            if (notScheduled.Count > 0)
            {
                var newSchedules = notScheduled.Select(u => new UpdateSchedule
                {
                    UserId = u,
                    Mode = Mode.Standard,
                    NextUpdate = DateTime.UtcNow,
                });
                dbContext.UpdateSchedules.AddRange(newSchedules);
                await dbContext.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        /***
         * 方便阅读
         */
        private static string GetTipMessage(BindingInfo? bid, bool netOk, UserInfo? user, int limitSum, double? bp1, int limitBp1)
        {
            string remark;
            if (bid == null)
            {
                remark = "未绑定";
            }
            else if (!netOk)
            {
                remark = "API 错误";
            }
            else if (user == null)
            {
                remark = "被 ban 了";
            }
            else if (user.Performance == 0)
            {
                remark = "可能不活跃";
            }
            else if (user.Performance >= limitSum)
            {
                remark = "超限";
            }
            else if (bp1 >= limitBp1)
            {
                remark = "超限BP1";
            }
            else
            {
                remark = "正常";
            }

            return remark;
        }

        public bool ShouldResponse(MessageContext context)
        {
            return context is GroupMessage g
                && g.GroupId == 管理群号
                && g.Content.TryGetPlainText(out string text)
                && RegexCommand(s_regex, text);
        }
    }
}
