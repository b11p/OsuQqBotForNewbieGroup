using System;
using System.Linq;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Osu.Newbie;
using Bleatingsheep.Osu.PerformancePlus;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;

namespace Bleatingsheep.NewHydrant.Osu
{
    [Function("pp+")]
    internal class PerformancePlusUser : OsuFunction, IMessageCommand
    {
        private static readonly PerformancePlusSpider s_spider = new PerformancePlusSpider();

        public IMessageCommand Create() => new PerformancePlusUser();
        private string queryUser;
        public async Task ProcessAsync(Sisters.WudiLib.Posts.Message message, HttpApiClient api)
        {


            if (message is GroupMessage grt && NewbieCardChecker.IgnoreListProvider.MonitoredGroups.Contains(grt.GroupId) && grt.Sender.Authority == Sisters.WudiLib.Responses.GroupMemberInfo.GroupMemberAuthority.Manager)
            {
                var (_, binding) = await DataProvider.GetBindingIdAsync(grt.UserId);
                if (binding != null)
                {
                    var (_, userInfo) = await OsuApi.GetUserInfoAsync(binding.Value, OsuMixedApi.Mode.Standard);
                    var hint = NewbieCardChecker.GetHintMessage(userInfo.Name, grt.Sender.InGroupName);
                    if (hint != null)
                    {
                        await api.SendMessageAsync(grt.Endpoint, "请管理员以身作则，不要改乱七八糟的群名片。");
                        return;
                    }
                }
            }

            dynamic query;
            if (!string.IsNullOrWhiteSpace(queryUser))
            {
                bool success;
                (success, query) = await GetUserKey(queryUser);
                if (!success)
                {
                    await api.SendMessageAsync(message.Endpoint, "查询失败。");
                    return;
                }
                if (query is null)
                {
                    await api.SendMessageAsync(message.Endpoint, "查无此人。");
                    return;
                }
            }
            else
            {
                var (success, userId) = await DataProvider.GetBindingIdAsync(message.UserId);
                if (!success)
                {
                    await api.SendMessageAsync(message.Endpoint, "查询绑定账号失败。");
                    return;
                }
                if (userId == null)
                {
                    await api.SendMessageAsync(message.Endpoint, "未绑定。请发送“绑定”后跟你的用户名进行绑定。");
                    return;
                }
                query = userId.Value;
            }
            try
            {
                var userPlus = (UserPlus)await s_spider.GetUserPlusAsync(query);
                if (userPlus == null)
                {
                    await api.SendMessageAsync(message.Endpoint, string.IsNullOrWhiteSpace(queryUser) ? "被办了。" : "查无此人。");
                    return;
                }

                var oldQuery = await Database.GetRecentPlusHistory(userPlus.Id);
                if (!oldQuery.Success)
                {
                    await api.SendMessageAsync(message.Endpoint, "无法找到历史数据。");
                    FLogger.LogException(oldQuery.Exception);
                }

                var old = oldQuery.EnsureSuccess().Result;

                var responseMessage = old == null
                    ? $@"{userPlus.Name} 的 PP+ 数据
Performance: {userPlus.Performance}
Aim (Jump): {userPlus.AimJump}
Aim (Flow): {userPlus.AimFlow}
Precision: {userPlus.Precision}
Speed: {userPlus.Speed}
Stamina: {userPlus.Stamina}
Accuracy: {userPlus.Accuracy}"
                    : $@"{userPlus.Name} 的 PP+ 数据
Performance: {userPlus.Performance}{userPlus.Performance - old.Performance: (+#); (-#); ;}
Aim (Jump): {userPlus.AimJump}{userPlus.AimJump - old.AimJump: (+#); (-#); ;}
Aim (Flow): {userPlus.AimFlow}{userPlus.AimFlow - old.AimFlow: (+#); (-#); ;}
Precision: {userPlus.Precision}{userPlus.Precision - old.Precision: (+#); (-#); ;}
Speed: {userPlus.Speed}{userPlus.Speed - old.Speed: (+#); (-#); ;}
Stamina: {userPlus.Stamina}{userPlus.Stamina - old.Stamina: (+#); (-#); ;}
Accuracy: {userPlus.Accuracy}{userPlus.Accuracy - old.Accuracy: (+#); (-#); ;}";
                if (message is GroupMessage g && g.GroupId == 758120648)
                {
                    //responseMessage += $"\r\n化学式没付钱指数：{C8Mod.CostOf(userPlus):0.0}";
                    responseMessage = C8Mod.ModPerformancePlus(responseMessage, old, userPlus);
                }
                await api.SendMessageAsync(message.Endpoint, responseMessage);

                if (old == null)
                {
                    var addResult = await Database.AddPlusHistoryAsync(userPlus);
                    if (!addResult.Success)
                        FLogger.LogException(addResult.Exception);
                }
            }
            catch (ExceptionPlus)
            {
                await api.SendMessageAsync(message.Endpoint, "查询PP+失败。");
                return;
            }
        }

        private static async Task<(bool success, dynamic key)> GetUserKey(string userName)
        {
            if (userName.All(c => char.IsDigit(c)))
            {
                var (success, user) = await OsuApi.GetUserInfoAsync(userName, OsuMixedApi.Mode.Standard);
                return (success, user?.Id);
            }
            else
            {
                return (true, userName);
            }
        }

        public bool ShouldResponse(Sisters.WudiLib.Posts.Message message)
        {
            if (!message.Content.IsPlaintext)
                return false;
            string text = message.Content.Text.Trim();
            if (text.StartsWith("+", StringComparison.Ordinal))
            {
                queryUser = text.Substring("+".Length).Trim();
                return string.IsNullOrEmpty(queryUser) || OsuHelper.IsUsername(queryUser);
            }
            return false;
        }
    }
}
