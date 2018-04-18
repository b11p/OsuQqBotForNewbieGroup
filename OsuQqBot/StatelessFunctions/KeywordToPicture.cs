using System.Collections.Generic;
using System.IO;
using System.Linq;
using OsuQqBot.QqBot;

namespace OsuQqBot.StatelessFunctions
{
    class KeywordToPicture : IStatelessFunction
    {
        private const string BasePath = @"C:\Users\Administrator\OneDrive - NTUA\Server\image\";

        private IDictionary<(long qq, string keyword), string> pics = new Dictionary<(long, string), string>()
        {
            { (995753021, "？？？"), "fl3l.png" },
            { (995753021, "???"), "fl3l.png" },
            { (2643555740, "菜"), "pata嘿菜鸡们.jpg" },
        };

        public bool ProcessMessage(EndPoint endPoint, MessageSource messageSource, string message)
        {
            var bot = OsuQqBot.QqApi;
            message = bot.AfterReceive(message);
            var ok = from p in pics
                     where p.Key.qq == messageSource.FromQq && message.Contains(p.Key.keyword)
                     select p.Value;
            foreach (var p in ok)
            {
                bot.SendMessageAsync(endPoint, bot.LocalImage(Path.Combine(BasePath, p)));
            }
            return ok.Any();
        }
    }
}
