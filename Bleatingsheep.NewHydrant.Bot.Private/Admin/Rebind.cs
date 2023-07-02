using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core;
using Bleatingsheep.NewHydrant.Data;
using Bleatingsheep.NewHydrant.Osu;
using Microsoft.Extensions.Logging;
using Sisters.WudiLib;

namespace Bleatingsheep.NewHydrant.Admin
{
    [Component("rebind")]
    class Rebind : Service, IMessageCommand
    {
        private readonly IOsuDataUpdator _osuDataUpdator;
        private readonly IDataProvider _dataProvider;
        private readonly ILogger<Rebind> _logger;

        public Rebind(OsuMixedApi.OsuApiClient osuApi, IOsuDataUpdator osuDataUpdator, IDataProvider dataProvider, ILogger<Rebind> logger)
        {
            OsuApi = osuApi;
            _osuDataUpdator = osuDataUpdator;
            _dataProvider = dataProvider;
            _logger = logger;
        }

        private static IVerifier Verifier { get; } = new AdminVerifier();
        private OsuMixedApi.OsuApiClient OsuApi { get; }

        private long _qq;
        private string _username;
        private long _operator;
        private string _reason;

        public async Task ProcessAsync(Sisters.WudiLib.Posts.Message message, HttpApiClient api)
        {
            // 验证是否具有改绑权限
            if (!await Verifier.IsAdminAsync(message.UserId))
            {
                await api.SendMessageAsync(message.Endpoint, "你没有更改绑定的权限。如果你是管理成员，请查看管理群公告获取权限。");
                return;
            }

            var osuApi = OsuApi;

            string operatorName;
            try
            {  // 获取操作者信息。
                var operatorBind = await _dataProvider.GetOsuIdAsync(_operator).ConfigureAwait(false);
                if (operatorBind == null)
                {
                    await api.SendMessageAsync(message.Endpoint, "操作者未绑定 osu! 账号，请先为自己绑定。").ConfigureAwait(false);
                    return;
                }
                var (succeed, operatorUserInfo) = await osuApi.GetUserInfoAsync(operatorBind.Value, OsuMixedApi.Mode.Standard).ConfigureAwait(false);
                if (!succeed)
                {
                    await api.SendMessageAsync(message.Endpoint, "访问 osu! API 失败。").ConfigureAwait(false);
                    return;
                }
                if (operatorUserInfo == null)
                {
                    await api.SendMessageAsync(message.Endpoint, "无法获取到操作者的用户名，可能是因为操作者被 ban 了。").ConfigureAwait(false);
                    return;
                }
                operatorName = operatorUserInfo.Name;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "重新绑定时，获取操作者信息失败。");
                await api.SendMessageAsync(message.Endpoint, "查询操作者信息失败，请稍后再尝试。").ConfigureAwait(false);
                return;
            }

            // 获取此用户名的相关信息。
            var (networkSuccess, newUser) = await osuApi.GetUserInfoAsync(_username, OsuMixedApi.Mode.Standard);
            if (!networkSuccess)
            {
                await api.SendMessageAsync(message.Endpoint, "网络访问失败。");
                return;
            }
            ExecutingException.Cannot(newUser == null, "没有这个用户。");

            // 绑定。
            var (isChanged, oldOsuId, _) = await _osuDataUpdator.AddOrUpdateBindingInfoAsync(
                qq: _qq,
                osuId: newUser.Id,
                osuName: newUser.Name,
                source: "由管理员修改",
                operatorId: _operator,
                operatorName: operatorName,
                reason: _reason,
                true
            ).ConfigureAwait(false);

            if (!isChanged)
            {
                await api.SendMessageAsync(message.Endpoint, "未更改绑定，因为已经绑定了该账号。").ConfigureAwait(false);
                return;
            }

            SendingMessage message1 = new SendingMessage("将") + SendingMessage.At(_qq) + new SendingMessage($"绑定为{newUser.Name}。");
            if (oldOsuId == null)
            {
                await api.SendMessageAsync(message.Endpoint, message1);
                return;
            }

            // 获取旧的用户信息。
            OsuMixedApi.UserInfo oldUser;
            (networkSuccess, oldUser) = await osuApi.GetUserInfoAsync(oldOsuId.Value, OsuMixedApi.Mode.Standard);
            if (!networkSuccess) message1 += new SendingMessage($"因网络问题，无法获取旧的用户名（id: {oldOsuId}）。");
            else if (oldUser == null) message1 += new SendingMessage($"以前绑定的用户已经被 Ban（id: {oldOsuId}）。");
            else message1 += new SendingMessage($"取代{oldUser.Name}({oldUser.Id})。");
            await api.SendMessageAsync(message.Endpoint, message1);
        }

        public bool ShouldResponse(Sisters.WudiLib.Posts.Message message)
        {
            _operator = message.UserId;

            var content = message.Content;
            var contentList = content.Sections;
            if (contentList.Count < 3) return false;
            if (contentList[0].TryGetText(out string p1)
                && "绑定".Equals(p1?.Trim(), StringComparison.InvariantCultureIgnoreCase)
                && contentList[1].TryGetAtMember(out _qq)
                && contentList[2].TryGetText(out string rebindInfo))
            {
                rebindInfo = rebindInfo.Trim();
                var match = Regex.Match(
                    input: rebindInfo,
                    pattern: "^(" + OsuHelper.UsernamePattern + @")\s*[:：]\s*(.+?)$"
                );
                if (!match.Success) return false;
                _username = match.Groups[1].Value;
                _reason = match.Groups[2].Value;
                return true;
            }
            return false;
        }
    }
}
