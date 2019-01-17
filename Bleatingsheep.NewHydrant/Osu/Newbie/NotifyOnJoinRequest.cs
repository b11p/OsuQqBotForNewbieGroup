using System.Collections.Generic;
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
        private static readonly IReadOnlyCollection<long> ManagedGroups = new List<long>
        {
            885984366,
            758120648,
            514661057,
        }.AsReadOnly();
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

        private static async Task HintBinding(HttpApiClient api, Endpoint endpoint, long userId)
        {
            var (success, osuId) = await DataProvider.GetBindingIdAsync(userId);
            string response = string.Empty;
            if (!success)
            {
                response = "查询失败";
            }

            else if (osuId == null)
            {
                response = "这个人没绑定。其余信息的查询正在施工。";
            }
            else
            {
                var (osuApiGood, user) = await OsuApi.GetUserInfoAsync(osuId.Value, OsuMixedApi.Mode.Standard);

                response = $"这个人绑定的 uid 是 {osuId}，用户名是 {(osuApiGood ? user?.Name ?? "被办了" : "查询失败")}\r\n（正在施工）";
            }
            await api.SendMessageAsync(endpoint, response);
        }

        public bool ShouldResponse(Sisters.WudiLib.Posts.Message message)
        {
            if (!(message is GroupMessage g && g.GroupId == NotifyOnJoinRequest.NewbieManagementGroupId && g.UserId == 3082577334))
                return false;

            _match = regex.Match(message.Content.Text);
            return _match.Success;
        }

        public static GroupRequestResponse Monitor(HttpApiClient httpApiClient, GroupRequest e)
        {

            _ = HintBinding(httpApiClient, new GroupEndpoint(NewbieManagementGroupId), e.UserId);
            return null;
        }
    }
}
