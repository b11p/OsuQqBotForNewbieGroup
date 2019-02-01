using NLog;
using OsuQqBot.QqBot;
using System;
using System.Linq;

namespace OsuQqBot.StatelessFunctions
{
    class Konachan : IStatelessFunction
    {
        protected virtual string Website { get; } = "https://konachan.net";
        protected virtual string StartWord { get; } = "konachan";

        /// <summary>
        /// 已知的最小的无法发送的大小。
        /// </summary>
        private const long MinNo = 11457410;

        private const long MaxYes = 2550804;

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

            if (_count > 1 && !IsVip(_qq) && !(endPoint is PrivateEndPoint))
            {
                qq.SendMessageAsync(endPoint, "这个功能是仅限私聊的。");
                return;
            }

            var k = new Moebooru.Api(Website);
            var result = (await k.PopularRecentAsync())?.Where(p => p.JpegSizeFallback < MinNo).ToList();
            var logger = LogManager.GetCurrentClassLogger();
            if (!result.Any())
            {
                logger.Debug("没有图。");
                return;
            }

            var posts = result.Take(_count);
            foreach (var post in posts)
            {
                logger.Debug($"{Website} | {post.id}, jpeg size {post.jpeg_file_size}({post.file_size})");
                qq.SendMessageAsync(endPoint, qq.OnlineImage(post.JpegUrl));
            }
        }
    }
}
