using System.Threading;
using OsuQqBot.QqBot;

namespace OsuQqBot.StatelessFunctions
{
    class KjMeimeiTime : IStatelessFunction
    {
        public bool ProcessMessage(EndPoint endPoint, MessageSource messageSource, string message)
        {
            if (!(endPoint is GroupEndPoint g)) return false;
            if (g.GroupId != 641236878) return false;
            if (messageSource.FromQq == kjBotId)
            {
                KjMode(g, message);
                return false;
            }
            if (!IsTimeCommand(message)) return false;
            LastTime(messageSource.FromQq);
            return false;
        }

        private const long kjId = 919815238;
        private const long kjBotId = 2839098896;
        /// <summary>
        /// 上次使用“!time”命令的人。
        /// </summary>
        private static long lastTime;

        private static long LastTime() => Interlocked.Read(ref lastTime);

        private static long LastTime(long user) => Interlocked.Exchange(ref lastTime, user);

        private static bool IsTimeCommand(string message)
        {
            if (!message.StartsWith("!")) return false;
            return message.Substring(1).Trim() == "time";
        }

        private static void KjMode(GroupEndPoint groupEndPoint, string message)
        {
            if (LastTime() != kjId) return;
            if (!message.Contains("提督")) return;
            LastTime(0);
            var api = OsuQqBot.QqApi;
            api.SendMessageAsync(groupEndPoint, message.Replace("提督", "kj妹妹"));
        }
    }
}
