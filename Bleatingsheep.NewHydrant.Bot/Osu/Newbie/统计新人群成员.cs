using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.Osu;
using Newtonsoft.Json;
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
            var results = new ConcurrentBag<string>();
            Parallel.ForEach(infoList, i =>
            {
                var (success, result) = DataProvider.GetBindingIdAsync(i.UserId).GetAwaiter().GetResult();
                if (!success)
                {
                    results.Add($"{i.UserId},,,,{JsonConvert.SerializeObject(i.DisplayName)},网络错误");
                }
                else if (result is null)
                {
                    results.Add($"{i.UserId},,,,{JsonConvert.SerializeObject(i.DisplayName)},未绑定");
                }
                else
                {
                    var (apiSuccess, user) = OsuApi.GetUserInfoAsync(result.Value, Mode.Standard).GetAwaiter().GetResult();
                    if (!apiSuccess)
                    {
                        results.Add($"{i.UserId},,,,{JsonConvert.SerializeObject(i.DisplayName)},API错误");
                    }
                    else if (user is null)
                    {
                        results.Add($"{i.UserId},,,,{JsonConvert.SerializeObject(i.DisplayName)},被ban了");
                    }
                    else
                    {
                        results.Add($"{i.UserId},{result},{user.Name},{user.Performance},{JsonConvert.SerializeObject(i.DisplayName)}," +
                            $"{(user.Performance == 0 ? "可能不活跃" : (user.Performance >= limit ? "超限" : "正常"))}");
                    }
                }
            });
            var message = string.Join("\r\n", results.Prepend("qq,uid,name,pp,card,remark"));
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
