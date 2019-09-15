using System.Collections.Concurrent;
using System.IO;
using System.Linq;
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
    [Function("newbie_statistics")]
    public class 统计新人群成员 : OsuFunction, IMessageCommand
    {
        private const int 新人群号 = 885984366;
        private const int 管理群号 = 695600319;
        private const int Limit = 2800;
        private const string DestPath = "/root/outputs/statistics.csv";
        private const string ResourceUrl = "https://res.bleatingsheep.org/statistics.csv";

        public async Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            await api.SendMessageAsync(context.Endpoint, $"即将统计新人群玩家列表，PP 超过 {Limit} 将标记为超限。");
            var infoList = await api.GetGroupMemberListAsync(新人群号);
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
                            $"{(user.Performance == 0 ? "可能不活跃" : (user.Performance >= Limit ? "超限" : "正常"))}");
                    }
                }
            });
            var message = string.Join("\r\n", results.Prepend("qq,uid,name,pp,card,remark"));
            Logger.Info(message);
            File.WriteAllText(DestPath, message, new System.Text.UTF8Encoding(true));
            await api.SendMessageAsync(context.Endpoint, $"统计完成，前往 {ResourceUrl} 查看结果。");
        }

        public bool ShouldResponse(MessageContext context)
        {
            return context is GroupMessage g
                && g.GroupId == 管理群号
                && g.Content.TryGetPlainText(out string text)
                && text.Trim() == "统计新人群玩家";
        }
    }
}
