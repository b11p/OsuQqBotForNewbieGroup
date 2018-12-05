using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core;
using Bleatingsheep.OsuMixedApi;
using Bleatingsheep.OsuQqBot.Database.Execution;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using Sisters.WudiLib.Responses;

namespace Bleatingsheep.NewHydrant.Osu.Newbie
{
    [Function("newbie_keeper")]
    internal class NewbieKeeper : IMessageMonitor
    {
        private static TimeSpan CheckInterval { get; } = new TimeSpan(10, 53, 31);

        private readonly INewbieInfoProvider IgnoreListProvider = HardcodedProvider.GetProvider();

        private readonly object _thisLock = new object();
        private readonly Dictionary<(long group, long qq), DateTime> _lastCheckTime = new Dictionary<(long group, long qq), DateTime>();

        public async Task OnMessageAsync(Sisters.WudiLib.Posts.Message message, HttpApiClient api, ExecutingInfo executingInfo)
        {
            var g = message as GroupMessage;
            // 检查群是否要监视。
            ExecutingException.Ensure(string.Empty,
                g != null
                && IgnoreListProvider.MonitoredGroups.Contains(g.GroupId)
            );

            // 检查上次检查时间。
            lock (_thisLock)
            {
                var hasChecked = _lastCheckTime.TryGetValue((g.GroupId, g.UserId), out var lastCheck);
                if (hasChecked && DateTime.Now - lastCheck < CheckInterval)
                    return;
                _lastCheckTime[(g.GroupId, g.UserId)] = DateTime.Now;
            }

            // 检查是否在忽略列表里。
            if (await IgnoreListProvider.ShouldIgnoreAsync(g.UserId))
                return;

            // 获取绑定的 osu! 游戏账号。
            var (success, osuId) = await executingInfo.Data.GetBindingIdAsync(g.UserId);
            ExecutingException.Ensure(success, string.Empty);
            if (osuId == null)
            {// 在没有绑定的情况下尝试自动绑定。
                string response;
                response = await AutoBind(api, executingInfo, g, success);
                await api.SendGroupMessageAsync(g.GroupId, SendingMessage.At(g.UserId) + new SendingMessage("\r\n您好，" + response));
                return;
            }

            // 获取游戏账号信息。
            UserInfo userInfo;
            (success, userInfo) = await executingInfo.OsuApi.GetUserInfoAsync(osuId.Value, Mode.Standard);
            ExecutingException.Ensure(string.Empty, success);

            // 获取群员信息。
            GroupMemberInfo groupMember = null;
            try
            {
                groupMember = await api.GetGroupMemberInfoAsync(g.GroupId, g.UserId);
            }
            catch (ApiAccessException)
            {
                return;
            }

            if (groupMember == null)
                return;

            // 检查用户名。
            var mother = await executingInfo.MotherShipApi.GetUserInfoAsync(g.UserId);
            if (mother?.Data != null)
                await CheckGroupCard(api, groupMember, g, userInfo?.Name ?? mother.Data.Name);

            ExecutingException.Ensure(userInfo != null, string.Empty);
            // 检查 PP。
            await CheckPerformance(api, groupMember, g, userInfo);

        }

        private static readonly HashSet<GroupMemberInfo.GroupMemberAuthority> AcceptedIgnore = new HashSet<GroupMemberInfo.GroupMemberAuthority>
        {
            GroupMemberInfo.GroupMemberAuthority.Leader,
            GroupMemberInfo.GroupMemberAuthority.Manager
        };

        private async Task CheckPerformance(HttpApiClient api, GroupMemberInfo groupMember, GroupMessage g, UserInfo userInfo)
        {
            if (AcceptedIgnore.Contains(groupMember.Authority)
                || await IgnoreListProvider.ShouldIgnorePerformanceAsync(g.GroupId, g.UserId))
                return;

            // 获取 PP 限制。
            double? performanceLimit = IgnoreListProvider.PerformanceLimit(g.GroupId);

            if (userInfo.Performance > performanceLimit)
            {
                await api.SendGroupMessageAsync(g.GroupId, SendingMessage.At(g.UserId) + new SendingMessage(" 您的PP超限，即将离开本群。"));
            }
        }

        private static async Task CheckGroupCard(HttpApiClient api, GroupMemberInfo groupMember, GroupMessage g, string name)
        {
            string card = string.IsNullOrEmpty(groupMember.InGroupName) ? groupMember.Nickname : groupMember.InGroupName;
            if (OsuHelper.DiscoverUsernames(card).Any(u => u.Equals(name, StringComparison.OrdinalIgnoreCase)))
                return;
            string hint;
            // 用户名不行。
            if (card.Contains(name, StringComparison.OrdinalIgnoreCase))
            {
                // 临时忽略。
                hint = "建议修改群名片，不要在用户名前后添加可以被用做用户名的字符，以免混淆。";
                hint += "\r\n" + "建议群名片：" + RecommendCard(card, name);
            }
            else
            {
                hint = "为了方便其他人认出您，请修改群名片，必须包括正确的 osu! 用户名。";
            }
            await api.SendGroupMessageAsync(g.GroupId, SendingMessage.At(g.UserId) + new SendingMessage($"\r\n{name}，您好。" + hint));
        }

        private static async Task<string> AutoBind(HttpApiClient api, ExecutingInfo executingInfo, GroupMessage g, bool success)
        {
            string response;
            var memberInfo = await api.GetGroupMemberInfoAsync(g.GroupId, g.UserId);

            if (memberInfo == null)
            {
                // TODO
                ExecutingException.Ensure(false, "");
            }

            var card = memberInfo.InGroupName;
            if (card == null)
                card = string.Empty;
            var usernames = OsuHelper.DiscoverUsernames(card);
            var validUsers = new List<UserInfo>();
            foreach (var username in usernames)
            {
                UserInfo info;
                (success, info) = await executingInfo.OsuApi.GetUserInfoAsync(username, Mode.Standard);
                ExecutingException.Ensure(success, string.Empty);
                if (info != null)
                    validUsers.Add(info);
            }
            if (validUsers.Count > 1)
            {
                response = "请修改群名片，除 osu! 游戏用户名外不要包含其他信息，以便自动绑定。";
            }
            else if (validUsers.Count < 1)
            {
                response = "请修改群名片，必须包含 osu! 游戏用户名，以方便与群友互相认识。";
            }
            else
            {
                (await executingInfo.Database.AddNewBindAsync(g.UserId, validUsers[0].Id, validUsers[0].Name, "Auto", null, null) as IExecutingResult<object>).EnsureSuccess();
                response = $"欢迎来到新人群，已自动绑定 osu! 游戏账号 {validUsers[0].Name}。";
            }

            return response;
        }

        /// <summary>
        /// 根据群名片和用户名推荐群名片
        /// </summary>
        private static string RecommendCard(string card, string username)
        {
            int firstIndex = card.IndexOf(username, StringComparison.OrdinalIgnoreCase);
            if (firstIndex != -1)
            {
                string recommendCard = card.Substring(0, firstIndex);
                if (firstIndex != 0)
                    recommendCard += "|";
                recommendCard += username;
                if (firstIndex + username.Length < card.Length)
                {
                    recommendCard += "|" + card.Substring(firstIndex + username.Length);
                }
                return recommendCard;
            }
            return null;
        }
    }
}
