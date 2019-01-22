//using System;
//using System.Threading.Tasks;
//using Bleatingsheep.NewHydrant.Attributions;
//using Bleatingsheep.NewHydrant.Core;
//using Bleatingsheep.Osu.ApiV2;
//using Sisters.WudiLib;

//namespace Bleatingsheep.NewHydrant.Osu
//{
//    [Function("play_time")]
//    internal class PlayTimeQuery : IMessageCommand
//    {
//        public async Task ProcessAsync(Sisters.WudiLib.Posts.Message message, HttpApiClient api, ExecutingInfo executingInfo)
//        {
//            var (success, result) = await executingInfo.Data.GetBindingIdAsync(message.UserId);
//            ExecutingException.Ensure(success, "哎，获取绑定信息失败了。");
//            ExecutingException.Ensure(result != null, "没绑定！");
//            var (status, userv2) = await ApiV2.Client.GetUserAsync(result.Value, Mode.Osu);
//            ExecutingException.Ensure(status == ApiStatus.Success, "我感到一股危机。");

//            var stat = userv2.Statistics;
//            var playTime = stat.PlayTime;
//            var replyContent = GetDisplayTime(playTime);
//            await api.SendMessageAsync(message.Endpoint, replyContent);
//        }

//        public static string GetDisplayTime(TimeSpan playTime)
//        {
//            var result = string.Empty;

//            void Append(string s)
//            {
//                if (result.Length != 0)
//                    result += " ";
//                result += s;
//            }

//            if (playTime.Days * 24 + playTime.Hours != 0)
//                Append($"{playTime.Days * 24 + playTime.Hours} 小时");
//            if (playTime.Minutes != 0)
//                Append($"{playTime.Minutes} 分钟");
//            if (playTime.Seconds != 0 || result.Length == 0)
//                Append($"{playTime.Seconds} 秒");
//            return result;
//        }

//        public bool ShouldResponse(Sisters.WudiLib.Posts.Message message) => message.Content.IsPlaintext && message.Content.Text.Trim().Equals("playtime", StringComparison.InvariantCultureIgnoreCase);
//    }
//}
