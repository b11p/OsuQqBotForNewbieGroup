using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;

namespace Bleatingsheep.NewHydrant.Osu.Newbie
{
    [Function("newbie_request_notify")]
    class NotifyOnJoinRequest : OsuFunction, IMessageCommand
    {
        private const string Pattern = @"^收到新人群加群申请\r\n群号: (\d+)\r\n群类型: .*?\r\n申请者: (\d+)\r\n验证信息: (.*)$"; // 匹配上报申请的消息。
        private readonly static Regex regex = new Regex(Pattern, RegexOptions.Compiled);

        private Match _match;
        private long GroupId => long.Parse(_match.Groups[1].Value);
        private long UserId => long.Parse(_match.Groups[2].Value);
        private string Message => _match.Groups[3].Value;

        public async Task ProcessAsync(Sisters.WudiLib.Posts.Message message, HttpApiClient api)
        {
            var (success, osuId) = await DataProvider.GetBindingIdAsync(UserId);
            if (!success)
                return;

            if (osuId == null)
                await api.SendMessageAsync(message.Endpoint, "这个人没绑定。其余信息的查询正在施工。");
            else
                await api.SendMessageAsync(message.Endpoint, $"这个人绑定的 uid 是 {osuId}。\r\n（正在施工）");
        }

        public bool ShouldResponse(Sisters.WudiLib.Posts.Message message)
        {
            if (!(message is GroupMessage g && g.GroupId == 695600319 && g.UserId == 3082577334))
                return false;

            _match = regex.Match(message.Content.Text);
            return _match.Success;
        }
    }
}
