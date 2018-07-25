﻿using System;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core;
using Bleatingsheep.NewHydrant.Osu;
using Sisters.WudiLib;

namespace Bleatingsheep.NewHydrant.Admin
{
    [Function("rebind")]
    class Rebind : IInitializable, IMessageCommand
    {
        private static IVerifier Verifier;

        public string Name { get; } = null;

        public IMessageCommand Create() => new Rebind();

        public Task<bool> InitializeAsync(ExecutingInfo executingInfo)
        {
            Verifier = new AdminVerifier(executingInfo);
            return new Task<bool>(() => true);
        }

        private long _qq;
        private string _username;
        private long _operator;

        public async Task ProcessAsync(Sisters.WudiLib.Posts.Message message, HttpApiClient api, ExecutingInfo executingInfo)
        {
            var osuApi = executingInfo.OsuApi;

            // 获取操作者信息。
            var operatorBind = await executingInfo.Database.GetBindingIdAsync(_operator);
            if (!operatorBind.Success)
            {
                await api.SendMessageAsync(message.Endpoint, "查询数据库失败，无法记录日志。");
                return;
            }
            string operatorName = (await osuApi.GetUserInfoAsync(operatorBind.Result.Value, OsuMixedApi.Mode.Standard)).Item2?.Name ?? "未知";

            // 获取此用户名的相关信息。
            var (networkSuccess, newUser) = await osuApi.GetUserInfoAsync(_username, OsuMixedApi.Mode.Standard);
            if (!networkSuccess)
            {
                await api.SendMessageAsync(message.Endpoint, "网络访问失败。");
                return;
            }
            if (newUser == null)
            {
                await api.SendMessageAsync(message.Endpoint, "没有这个用户。");
                return;
            }

            // 绑定。
            var oldBind = (await executingInfo.Database.ResetBindingAsync(
                qq: _qq,
                osuId: newUser.Id,
                osuName: newUser.Name,
                source: "由管理员修改",
                operatorId: _operator,
                operatorName: operatorName
            )).EnsureSuccess("绑定失败，数据库访问出错。");

            SendingMessage message1 = new SendingMessage("将") + SendingMessage.At(_qq) + new SendingMessage($"绑定为{newUser.Name}。");
            if (oldBind.Result == null)
            {
                await api.SendMessageAsync(message.Endpoint, message1);
                return;
            }

            // 获取旧的用户信息。
            OsuMixedApi.UserInfo oldUser;
            (networkSuccess, oldUser) = await osuApi.GetUserInfoAsync(oldBind.Result.Value, OsuMixedApi.Mode.Standard);
            if (!networkSuccess) message1 += new SendingMessage($"因网络问题，无法获取旧的用户名（id: {oldBind.Result.Value}）。");
            else if (oldUser == null) message1 += new SendingMessage($"以前绑定的用户已经被 Ban（id: {oldBind.Result.Value}）。");
            else message1 += new SendingMessage($"取代{oldUser.Name}({oldUser.Id})。");
            await api.SendMessageAsync(message.Endpoint, message1);
        }

        public bool ShouldResponse(Sisters.WudiLib.Posts.Message message)
        {
            if (!Verifier.IsAdminAsync(message.UserId).Result) return false;
            _operator = message.UserId;

            var content = message.Content;
            var contentList = content.GetSections();
            if (contentList.Count < 3) return false;
            if (contentList[0].TryGetText(out string p1)
                && "绑定".Equals(p1?.Trim(), StringComparison.InvariantCultureIgnoreCase)
                && contentList[1].TryGetAtMember(out _qq)
                && contentList[2].TryGetText(out _username))
            {
                _username = _username.Trim();
                return OsuHelper.IsUsername(_username);
            }
            return false;
        }
    }
}
