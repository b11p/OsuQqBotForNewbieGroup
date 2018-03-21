using OsuQqBot.QqBot;
using System.Linq;

namespace OsuQqBot.StatelessFunctions
{
    class Konachan : IStatelessFunction
    {
        private const string website = "https://konachan.net";

        public bool ProcessMessage(EndPoint endPoint, MessageSource messageSource, string message)
        {
            if (message.ToLowerInvariant() != "konachan") return false;
            ProcessAsync(endPoint, message);
            return true;
        }

        private async void ProcessAsync(EndPoint endPoint, string message)
        {
            var k = new Moebooru.Api(website);
            var (result, info) = await k.PopularRecentDebugAsync();
            var qq = OsuQqBot.QqApi;
            qq.SendMessageAsync(endPoint, qq.BeforeSend(info));
            if (!result.Any()) return;
            var post = result.First();
            qq.SendMessageAsync(endPoint, qq.OnlineImage(post.FileUrl));
        }
    }
}
