using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
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
    //[Function("check_over_pp")]
    public class 检查新人群超PP : OsuFunction, IMessageCommand
    {
        public async Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            var infoList = await api.GetGroupMemberListAsync(885984366);
            var results = new ConcurrentBag<string>();
            results.Add("qq,uid,name,pp,card,remark");
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
                            $"{(user.Performance == 0 ? "可能不活跃" : (user.Performance >= 3100 ? "超限" : "正常"))}");
                    }
                }
            });
            var message = string.Join("\r\n", results);
            Logger.Info(message);
            await api.SendMessageAsync(context.Endpoint, message);
        }

        public bool ShouldResponse(MessageContext context)
        {
            return context is GroupMessage g
                && g.GroupId == 695600319
                && g.Content.TryGetPlainText(out string text)
                && text.Trim() == "超限";
        }
    }
}
