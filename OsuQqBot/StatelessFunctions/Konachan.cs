using OsuQqBot.QqBot;
using System;
using System.Linq;

namespace OsuQqBot.StatelessFunctions
{
    class Konachan : IStatelessFunction
    {
        protected virtual string Website { get; } = "https://konachan.net";
        protected virtual string StartWord { get; } = "konachan";

        private static bool IsVip(long qq) => qq == 630060047;

        private long _qq;
        private int _count;

        public bool ProcessMessage(EndPoint endPoint, MessageSource messageSource, string message)
        {
            if (!message.StartsWith(StartWord, StringComparison.InvariantCultureIgnoreCase))
                return false;

            _qq = messageSource.FromQq;

            ProcessAsync(endPoint, message);
            return true;
        }

        private async void ProcessAsync(EndPoint endPoint, string message)
        {
            var qq = OsuQqBot.QqApi;
            var count = message.Substring(StartWord.Length);
            if (!int.TryParse(count, out _count))
            {
                _count = 1;
            }

            if (_count > 1 && !IsVip(_qq))
            {
                qq.SendMessageAsync(endPoint, "不给看。");
                return;
            }

            var k = new Moebooru.Api(Website);
            var result = await k.PopularRecentAsync();
            if (!result.Any())
                return;

            var posts = result.Take(_count);
            foreach (var post in posts)
            {
                qq.SendMessageAsync(endPoint, qq.OnlineImage(post.JpegUrl));
            }
        }
    }
}
