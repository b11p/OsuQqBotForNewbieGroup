﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Data;
using Bleatingsheep.Osu;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.Osu.Newbie
{
    [Component("newbie_statistics")]
    public class 统计新人群成员 : OsuFunction, IMessageCommand
    {
        private const int 管理群号 = 695600319;
        private const string DestPath = "/root/outputs/statistics_{0}.csv";
        private const string ResourceUrl = "https://res.bleatingsheep.org/statistics_{0}.csv";

        private static readonly Dictionary<string, (long, int)> s_groups = new()
        {
            { "新人群", (885984366, 2800) },
            { "进阶群", (928936255, 4500) },
        };

        private static readonly Regex s_regex = new Regex("^统计(?<group>.+?)玩家$", RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private readonly Lazy<NewbieContext> _lazyContext;
        private readonly Lazy<IDataProvider> _dataProvider;

        public 统计新人群成员(Lazy<NewbieContext> newbieContext, Lazy<IDataProvider> dataProvider)
        {
            _lazyContext = newbieContext;
            _dataProvider = dataProvider;
        }

        [Parameter("group")]
        public string ProcessingGroupName { get; set; }

        public Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            if (s_groups.TryGetValue(ProcessingGroupName, out var t))
            {
                var (groupId, limit) = t;
                return AnalyzeGroupMember(context, api, limit, groupId);
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        private async Task AnalyzeGroupMember(MessageContext context, HttpApiClient api, int limit, long groupId)
        {
            await api.SendMessageAsync(context.Endpoint, $"即将统计新人群玩家列表，PP 超过 {limit} 将标记为超限。");
            var infoList = await api.GetGroupMemberListAsync(groupId);
            Logger.Info($"Find {infoList.Length} users.");
            var userIdList = infoList.Select(i => i.UserId).ToList();
            IQueryable<BindingInfo> bindingQuery = _lazyContext.Value.Bindings.Where(b => userIdList.Contains(b.UserId));
            Logger.Info(bindingQuery.ToQueryString());
            var bindingList = await bindingQuery.ToListAsync().ConfigureAwait(false);
            Logger.Info($"Find {bindingList.Count} binding info.");

            var userInfoTaskArray = bindingList.Select(async b =>
            {
                var o = b.OsuId;
                try
                {
                    var userInfo = await _dataProvider.Value.GetUserInfoRetryAsync(o, Mode.Standard).ConfigureAwait(false);
                    return (BindingInfo: b, Successful: true, UserInfo: userInfo);
                }
                catch (Exception)
                {
                    return (b, false, null);
                }
            }).ToArray();
            Logger.Info("Complete fetching user info.");
            await Task.WhenAll(userInfoTaskArray).ConfigureAwait(false);
            var userInfo = userInfoTaskArray.Select(t => t.Result);
            var results = from i in infoList
                          join u in userInfo
                              on i.UserId equals u.BindingInfo.UserId into validUsers
                          from v in validUsers.DefaultIfEmpty()
                          let b = v.BindingInfo
                          let netOk = v.Successful
                          let user = v.UserInfo
                          select new
                          {
                              qq = i.UserId,
                              uid = b?.OsuId,
                              name = user?.Name,
                              pp = user?.Performance,
                              card = i.DisplayName,
                              remark = b == null ? "未绑定" :
                                  !netOk ? "API 错误" :
                                  user == null ? "被 ban 了" :
                                  (user.Performance == 0 ? "可能不活跃" : (user.Performance >= limit ? "超限" : "正常")),
                          };
            var writer = new StringWriter();
            using (var csvWriter = new CsvHelper.CsvWriter(writer, CultureInfo.InvariantCulture))
                await csvWriter.WriteRecordsAsync(results).ConfigureAwait(false);
            var message = writer.ToString();
            Logger.Info(message);
            File.WriteAllText(string.Format(DestPath, groupId), message, new System.Text.UTF8Encoding(true));
            await api.SendMessageAsync(context.Endpoint, $"统计完成，前往 {string.Format(ResourceUrl, groupId)} 查看结果。");
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
