using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core;
using Bleatingsheep.Osu.ApiV2;
using Sisters.WudiLib;

namespace Bleatingsheep.NewHydrant.Osu
{
    [Function("play_time")]
    internal class PlayTimeQuery : IInitializable, IMessageCommand
    {
        private static OsuApiV2Client s_v2Client;

        public string Name => "up";

        public IMessageCommand Create() => new PlayTimeQuery();

        public async Task<bool> InitializeAsync(ExecutingInfo executingInfo)
        {
            var authPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..", "config", "authv2.txt");
            var lines = await File.ReadAllLinesAsync(authPath);
            if (lines.Length != 2) return false;
            string username = lines[0];
            string password = lines[1];
            s_v2Client = new OsuApiV2Client(username, password);
            return true;
        }

        public async Task ProcessAsync(Sisters.WudiLib.Posts.Message message, HttpApiClient api, ExecutingInfo executingInfo)
        {
            var (success, result) = await executingInfo.Data.GetBindingIdAsync(message.UserId);
            ExecutingException.Ensure(success, "哎，获取绑定信息失败了。");
            ExecutingException.Ensure(result != null, "没绑定！");
            var (status, userv2) = await s_v2Client.GetUserAsync(result.Value, Mode.Osu);
            ExecutingException.Ensure(status == ApiStatus.Success, "我感到一股危机。");

            var hours = Math.Floor(userv2.Statistics.PlayTime.TotalHours);
            var replyContent = $"{hours:#小时;？？？;不到一小时}";
            await api.SendMessageAsync(message.Endpoint, replyContent);
        }

        public bool ShouldResponse(Sisters.WudiLib.Posts.Message message) => message.Content.IsPlaintext && message.Content.Text.Trim().Equals("playtime", StringComparison.InvariantCultureIgnoreCase);
    }
}
