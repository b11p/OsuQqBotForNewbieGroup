using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core;
using Bleatingsheep.Osu.PerformancePlus;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;

namespace Bleatingsheep.NewHydrant.Osu
{
    [Function("pp+")]
    internal class PerformancePlusUser : IMessageCommand
    {
        private static readonly PerformancePlusSpider s_spider = new PerformancePlusSpider();

        public IMessageCommand Create() => new PerformancePlusUser();
        private string queryUser;
        public async Task ProcessAsync(Sisters.WudiLib.Posts.Message message, HttpApiClient api, ExecutingInfo executingInfo)
        {
            dynamic query;
            if (!string.IsNullOrWhiteSpace(queryUser))
            {
                query = queryUser;
            }
            else
            {
                var (success, userId) = await executingInfo.Data.GetBindingIdAsync(message.UserId);
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
                var userPlus = await s_spider.GetUserPlusAsync(query);
                if (userPlus == null)
                {
                    await api.SendMessageAsync(message.Endpoint, string.IsNullOrWhiteSpace(queryUser) ? "被办了。" : "查无此人。");
                    return;
                }

                var oldQuery = await executingInfo.Database.GetRecentPlusHistory(userPlus.Id);
                if (!oldQuery.Success)
                {
                    await api.SendMessageAsync(message.Endpoint, "无法找到历史数据。");
                    executingInfo.Logger.LogException(oldQuery.Exception);
                }

                var old = oldQuery.Result;

                await api.SendMessageAsync(message.Endpoint, old == null
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
Accuracy: {userPlus.Accuracy}{userPlus.Accuracy - old.Accuracy: (+#); (-#); ;}");

                if (old == null)
                {
                    var addResult = await executingInfo.Database.AddPlusHistoryAsync(userPlus);
                    if (!addResult.Success) executingInfo.Logger.LogException(addResult.Exception);
                }
            }
            catch (ExceptionPlus)
            {
                await api.SendMessageAsync(message.Endpoint, "查询PP+失败。");
                return;
            }
        }

        public bool ShouldResponse(Sisters.WudiLib.Posts.Message message)
        {
            if (!message.Content.IsPlaintext) return false;
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
