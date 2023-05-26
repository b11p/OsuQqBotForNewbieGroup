using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core;
using Bleatingsheep.NewHydrant.Data;
using Bleatingsheep.OsuMixedApi;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using Sisters.WudiLib.Responses;
using static Bleatingsheep.NewHydrant.Osu.Newbie.NewbieCardChecker;

namespace Bleatingsheep.NewHydrant.Osu.Newbie
{
    [Component("newbie_keeper")]
    internal class NewbieKeeper : Service, IMessageMonitor
    {
        private static TimeSpan CheckInterval { get; } = new TimeSpan(10, 53, 31);

        private static readonly object s_thisLock = new object();
        private static readonly Dictionary<(long group, long qq), DateTime> s_lastCheckTime = new Dictionary<(long group, long qq), DateTime>();

        private readonly IOsuDataUpdator _osuDataUpdator;

        public NewbieKeeper(ILegacyDataProvider dataProvider, OsuApiClient osuApi, IOsuDataUpdator osuDataUpdator)
        {
            DataProvider = dataProvider;
            OsuApi = osuApi;
            _osuDataUpdator = osuDataUpdator;
        }

        private ILegacyDataProvider DataProvider { get; }
        private OsuApiClient OsuApi { get; }

        public async Task OnMessageAsync(Sisters.WudiLib.Posts.Message message, HttpApiClient api)
        {
            var g = message as GroupMessage;
            // 检查群是否要监视。
            ExecutingException.Ensure(string.Empty,
                g != null
                && IgnoreListProvider.MonitoredGroups.Contains(g.GroupId)
            );

            // 检查上次检查时间。
            lock (s_thisLock)
            {
                var hasChecked = s_lastCheckTime.TryGetValue((g.GroupId, g.UserId), out var lastCheck);
                if (hasChecked && DateTime.Now - lastCheck < CheckInterval)
                    return;
                s_lastCheckTime[(g.GroupId, g.UserId)] = DateTime.Now;
            }

            // 检查是否在忽略列表里。
            if (await IgnoreListProvider.ShouldIgnoreAsync(g.UserId))
                return;

            // 获取绑定的 osu! 游戏账号。
            var (success, osuId) = await DataProvider.GetBindingIdAsync(g.UserId);
            ExecutingException.Ensure(success, string.Empty);
            if (osuId == null)
            {// 在没有绑定的情况下尝试自动绑定。
                string response;
                response = await AutoBind(api, g);
                if (!string.IsNullOrEmpty(response))
                {
                    await api.SendGroupMessageAsync(g.GroupId, SendingMessage.At(g.UserId) + new SendingMessage("\r\n您好，" + response));
                }
                return;
            }

            // 获取游戏账号信息。
            UserInfo userInfo;
            (success, userInfo) = await OsuApi.GetUserInfoAsync(osuId.Value, Mode.Standard);
            ExecutingException.Ensure(string.Empty, success);

            // TODO: 处理被 ban 的情况。
            if (userInfo is null) return;

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
            await CheckGroupCard(api, groupMember, g, userInfo.Name);

            ExecutingException.Ensure(userInfo != null, string.Empty);
            // 检查 PP。
            //await CheckPerformance(api, groupMember, g, userInfo);

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
            var card = string.IsNullOrEmpty(groupMember.InGroupName) ? groupMember.Nickname : groupMember.InGroupName;
            string hint;
            hint = NewbieCardChecker.GetHintMessage(name, card);
            if (hint != null)
                await api.SendGroupMessageAsync(g.GroupId, SendingMessage.At(g.UserId) + new SendingMessage($"\r\n{name}，您好。" + hint));
        }

        private async Task<string> AutoBind(HttpApiClient api, GroupMessage g)
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
                var (success, info) = await OsuApi.GetUserInfoAsync(username, Mode.Standard);
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
                var (isChanged, _, _) = await _osuDataUpdator.AddOrUpdateBindingInfoAsync(g.UserId, validUsers[0].Id, validUsers[0].Name, "Auto", null, null).ConfigureAwait(false);
                response = isChanged ? $"欢迎来到新人群，已自动绑定 osu! 游戏账号 {validUsers[0].Name}。" : "";
            }

            return response;
        }
    }
}
