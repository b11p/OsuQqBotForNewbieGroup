using System;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core;
using Bleatingsheep.Osu.ApiV2;
using Sisters.WudiLib;

namespace Bleatingsheep.NewHydrant.Osu
{
    [Function("play_time")]
    internal class PlayTimeQuery : IMessageCommand
    {
        public async Task ProcessAsync(Sisters.WudiLib.Posts.Message message, HttpApiClient api, ExecutingInfo executingInfo)
        {
            var (success, result) = await executingInfo.Data.GetBindingIdAsync(message.UserId);
            ExecutingException.Ensure(success, "哎，获取绑定信息失败了。");
            ExecutingException.Ensure(result != null, "没绑定！");
            var (status, userv2) = await ApiV2.Client.GetUserAsync(result.Value, Mode.Osu);
            ExecutingException.Ensure(status == ApiStatus.Success, "我感到一股危机。");

            var hours = Math.Floor(userv2.Statistics.PlayTime.TotalHours);
            var replyContent = $"{hours:#小时;？？？;不到一小时}";
            await api.SendMessageAsync(message.Endpoint, replyContent);
        }

        public bool ShouldResponse(Sisters.WudiLib.Posts.Message message) => message.Content.IsPlaintext && message.Content.Text.Trim().Equals("playtime", StringComparison.InvariantCultureIgnoreCase);
    }
}
