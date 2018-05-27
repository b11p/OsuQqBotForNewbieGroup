using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using OsuQqBot.QqBot;

namespace OsuQqBot.StatelessFunctions
{
    class CannotOverStar : IStatelessFunction
    {
        private const long group = 614892339;
        private const long bot = 1677323371;
        private const decimal limit = 5.4m;

        private const long admin = 962549599;

        private static readonly IReadOnlyList<long> managerList = new List<long>
        {
            962549599, // 咩咩
            2541721178, // heisiban
            546748348, // 化学式
            1012621328, // 咪咪
            2482000231, // 杰克王
            431600414, // 844
            630060047, // CYCLC
            1061566571, // dalou（本体）
        };

        private static long s_lastTalkManager = 546748348;
        private static long LastTalkManager
        {
            get => Interlocked.Read(ref s_lastTalkManager);
            set => Interlocked.Exchange(ref s_lastTalkManager, value);
        }

        private static readonly string img = @"C:\Users\Administrator\OneDrive - NTUA\Server\image\我真想禁你言.jpg";

        public bool ProcessMessage(EndPoint endPoint, MessageSource messageSource, string message)
        {
            var api = OsuQqBot.QqApi;
            message = api.AfterReceive(message);
            if (!(endPoint is GroupEndPoint g)) return false;
            if (g.GroupId != group) return false;
            UpdateLastTalk(messageSource.FromQq);

            if (messageSource.FromQq != bot) return false;
            string[] lines = message.Split("\r\n");
            int length = regex.Length;
            if (lines.Length != length) return false;

            decimal star = 0;
            int bid = 0;
            for (int i = 0; i < length; i++)
            {
                var match = regex[i].Match(lines[i]);
                if (!match.Success)
                {
                    NotifyFail(message, i);
                    return false;
                }
                if (i == 2)
                {
                    var bidstring = match.Groups[1].Value;
                    if (!int.TryParse(bidstring, out bid))
                    {
                        NotifyFail(message, i);
                    }
                }
                else if (i == 3)
                {
                    var starstring = match.Groups[1].Value;
                    if (!decimal.TryParse(starstring, out star))
                    {
                        NotifyFail(message, i);
                        return false;
                    }
                }
            }
            if (star > limit)
            {
                NotifyOverstar(g, star);
            }
            return false;
        }

        private void UpdateLastTalk(long fromQq)
        {
            if (managerList.Contains(fromQq)) LastTalkManager = fromQq;
        }

        private static void NotifyFail(string message, int i)
        {
            var api = OsuQqBot.QqApi;
            //api.SendPrivateMessageAsync(admin, "第" + (i + 1) + "行匹配失败");
            //api.SendPrivateMessageAsync(admin, message, true);
        }

        private static void NotifyOverstar(GroupEndPoint g, decimal star)
        {
            int minutes = (int)((star - limit) / 0.01m * 10);
            int hours = minutes / 60;
            minutes %= 60;
            var api = OsuQqBot.QqApi;
            string imgMessage = api.LocalImage(img);
            string hint = (hours != 0 ? hours + "h " : "") + (minutes != 0 ? minutes + "min " : "");
            api.SendMessageAsync(g, imgMessage + api.BeforeSend(hint));
            api.SendMessageAsync(g, api.At(LastTalkManager));
        }

        private static readonly Regex[] regex;
        static CannotOverStar()
        {
            regex = new Regex[9];
            regex[0] = new Regex(@"^.*? - .*? \[(?:\S|\S.*\S)]$", RegexOptions.Compiled); // 非贪婪匹配可能有性能提升，在不完全的测试中证实了这一点
            regex[1] = new Regex(@"^Beatmap by (?:\S|\S.*\S)$", RegexOptions.Compiled);
            regex[2] = new Regex(@"^https:\/\/osu.ppy.sh\/b\/(\d+)$", RegexOptions.Compiled);
            regex[3] = new Regex(@"^(?:Mods: \S+ )?Rank: (?:[ABCDSXF]|SH|XH) Star: (\d*\.\d{2})\*$", RegexOptions.Compiled);
            regex[4] = new Regex(@"^$", RegexOptions.Compiled);
            regex[5] = new Regex(@"^对比: \(现在 \/ fc \/ 98%\)$", RegexOptions.Compiled);
            regex[6] = new Regex(@"^\d{1,3}\.\d{2}% \/ \d{1,3}\.\d{2}% \/ 98\.00%$", RegexOptions.Compiled);
            regex[7] = new Regex(@"^\d*\.\d{2}pp \/ \d*\.\d{2}pp \/ \d*\.\d{2}pp$", RegexOptions.Compiled);
            regex[8] = new Regex(@"^\d+x \/ \d+x \/ \d+x$", RegexOptions.Compiled);
        }
    }
}
