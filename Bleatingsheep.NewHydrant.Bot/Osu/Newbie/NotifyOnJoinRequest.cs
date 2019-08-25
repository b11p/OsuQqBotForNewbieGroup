using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;

namespace Bleatingsheep.NewHydrant.Osu.Newbie
{
    [Function("newbie_request_notify")]
    class NotifyOnJoinRequest : OsuFunction, IMessageCommand
    {
        private const string Pattern = @"^收到新人群加群申请\r\n群号: (\d+)\r\n群类型: .*?\r\n申请者: (\d+)\r\n验证信息: (.*)$"; // 匹配上报申请的消息。
        private const int NewbieManagementGroupId = 695600319;
        private static readonly IReadOnlyDictionary<long, double?> ManagedGroups = new Dictionary<long, double?>
        {
            [885984366] = 2500,
            [758120648] = null,
            [514661057] = null,
        };
        private readonly static Regex regex = new Regex(Pattern, RegexOptions.Compiled);

        private Match _match;
        private long GroupId => long.Parse(_match.Groups[1].Value);
        private long UserId => long.Parse(_match.Groups[2].Value);
        private string Message => _match.Groups[3].Value;

        public async Task ProcessAsync(Sisters.WudiLib.Posts.Message message, HttpApiClient api)
        {
            Endpoint endpoint = message.Endpoint;
            var userId = UserId;
            await HintBinding(api, endpoint, userId);
        }

        private async Task ParseInfoAsync(
            HttpApiClient api,
            Endpoint sendBackEndpoint,
            long userId,
            int? osuId = null,
            OsuMixedApi.UserInfo userInfo = default,
            string comment = null)
        {
            var hints = new List<string>();
            var levelInfo = await api.GetLevelInfo(userId);
            if (levelInfo != null)
            {
                hints.Add($"QQ 等级为 {levelInfo.Level}");
            }
            if (!string.IsNullOrEmpty(comment))
            {
                var userNames = OsuHelper.DiscoverUsernames(comment).Where(n => !string.Equals(n, "osu", StringComparison.OrdinalIgnoreCase));
                if (userInfo != null && userNames.Any(n => string.Equals(userInfo.Name, n, StringComparison.OrdinalIgnoreCase)))
                {// 绑定一致

                }
                else if (osuId == null)
                {// 忽略已绑定的情况，因为可能绑定不一致或者查询失败。
                    foreach (var name in userNames)
                    {
                        var (success, info) = await OsuApi.GetUserInfoAsync(name, Bleatingsheep.Osu.Mode.Standard);
                        // 我想用 8.0 新语法
                        hints.Add($"{info?.Name ?? name}: " +
                            $"{(success ? info == null ? "不存在此用户。" : $"PP: {info.Performance}, PC: {info.PlayCount}, TTH: {info.TotalHits}" : "查询失败。")}");

                        // 提供绑定并放行的捷径。
                        if (info?.Performance < 2500)
                        {
                            var ms = new MemoryStream();
                            using (var bw = new BinaryWriter(ms, Encoding.UTF8, true))
                            {
                                bw.Write(userId);
                                bw.Write(info.Id);
                            }
                            ms.Write(MD5.Create().ComputeHash(ms.ToArray()));
                            var bytes = ms.ToArray();
                            var base64 = Convert.ToBase64String(bytes);
                            await api.SendMessageAsync(sendBackEndpoint, $"（占位）绑定为 {info.Name} 并放行：#{base64}#");
                        }
                    }
                }
            }
            if (hints.Any())
            {
                await api.SendMessageAsync(sendBackEndpoint, string.Join("\r\n", hints));
            }
        }

        private async ValueTask<double?> HintBinding(HttpApiClient api, Endpoint endpoint, long userId, string comment = null)
        {
            var (success, osuId) = await DataProvider.GetBindingIdAsync(userId);
            string response = string.Empty;
            double? performance = default;
            OsuMixedApi.UserInfo user = null;
            if (!success)
            {
                response = "查询失败";
            }

            else if (osuId == null)
            {
                response = "这个人没绑定。";
            }
            else
            {
                bool osuApiGood;
                (osuApiGood, user) = await OsuApi.GetUserInfoAsync(osuId.Value, OsuMixedApi.Mode.Standard);
                performance = user?.Performance;

                response = $"这个人绑定的 uid 是 {osuId}，用户名是 {(osuApiGood ? user?.Name ?? "被办了" : "查询失败")}\r\n（正在施工）";
            }

            // 提供额外信息
            try
            {
                await ParseInfoAsync(api, endpoint, userId, osuId, user, comment);
            }
            catch (Exception e)
            {
                await api.SendMessageAsync(endpoint, e.ToString());
            }

            // 保留 comment
            if (!string.IsNullOrEmpty(comment))
            {
                response = comment + "\r\n" + response;
            }

            await api.SendMessageAsync(endpoint, response);
            return performance;
        }

        public bool ShouldResponse(Sisters.WudiLib.Posts.Message message)
        {
            if (!(message is GroupMessage g && g.GroupId == NotifyOnJoinRequest.NewbieManagementGroupId && g.UserId == 3082577334))
                return false;

            _match = regex.Match(message.Content.Text);
            return _match.Success;
        }

        public GroupRequestResponse Monitor(HttpApiClient httpApiClient, GroupRequest e)
        {
            if (ManagedGroups.TryGetValue(e.GroupId, out var limit))
            {
                var endpoint = new GroupEndpoint(NewbieManagementGroupId);
                var performance = HintBinding(httpApiClient, endpoint, e.UserId, e.Comment).GetAwaiter().GetResult();
                if (performance >= limit)
                {
                    var reason = $"您的 PP 超限，不能加入本群。";
                    _ = httpApiClient.SendMessageAsync(endpoint, $"以“{reason}”拒绝。");
                    return new GroupRequestResponse(reason);
                    //_ = httpApiClient.SendMessageAsync(endpoint, $"建议拒绝。");
                    //return null;
                }
            }
            return null;
        }
    }
}
