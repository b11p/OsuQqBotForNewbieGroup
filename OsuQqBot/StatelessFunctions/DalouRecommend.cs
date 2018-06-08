using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OsuQqBot.QqBot;

namespace OsuQqBot.StatelessFunctions
{
    class DalouRecommend : IStatelessFunction
    {
        private long DalouBot => OsuQqBot.Daloubot;
        private const long Sheep = 962549599;
        private const long DebugGroup = 72318078;

        public bool ProcessMessage(EndPoint endPoint, MessageSource messageSource, string message)
        {
            if (messageSource.FromQq == DalouBot)
            {
                return DalouMessage(endPoint, message);
            }
            else if (messageSource.FromQq == Sheep)
            {
                return SheepMessage(endPoint, message);
            }
            else return false;
        }

        private bool SheepMessage(EndPoint endPoint, string message)
        {
            if (message == "Advanced Recommend")
            {
                OsuQqBot.QqApi.SendMessageAsync(endPoint, "!myid bleatingsheep");
                return true;
            }
            else if (message == "Filter")
            {
                OsuQqBot.QqApi.SendMessageAsync(endPoint, "!getmap");
            }
            else if (message == "Private Filter")
            {
                OsuQqBot.QqApi.SendPrivateMessageAsync(DalouBot, "!getmap");
            }
            return false;
        }

        private bool DalouMessage(EndPoint endPoint, string message)
        {
            var qq = OsuQqBot.QqApi;
            bool isPrivate = endPoint is PrivateEndPoint;
            if (isPrivate)
            {
                qq.SendGroupMessageAsync(DebugGroup, message);
                message = "[CQ:at,qq=122866607] \r\n" + message;
            }

            message = qq.AfterReceive(message);
            var messages = message.Split("\r\n");
            if (messages.Length <= 3) return false;
            if (messages[0] != "[CQ:at,qq=122866607] ") return false;
            if (messages[1] != "bleatingsheep的推荐图如下:") return false;
            if (messages[2] != "Bid, Mod, pp, 推荐指数") return false;

            var ps = new(int bid, bool hasHT, bool hasDT)[messages.Length - 3];

            for (int i = 3; i < messages.Length; i++)
            {
                var match = Regex.Match(messages[i], @"^(\d+), (.+?), .+?, .+$");
                if (!match.Success || !int.TryParse(match.Groups[1].Value, out int bid))
                {
                    qq.SendMessageAsync(endPoint, "失败，" + (i + 1));
                    if (isPrivate) qq.SendGroupMessageAsync(DebugGroup, "失败，" + (i + 1));
                    return false;
                }
                string mods = match.Groups[2].Value;
                bool hasHT = false;
                bool hasDT = false;
                if (mods.Contains("DT") || mods.Contains("HT")) hasDT = true;
                else if (mods.Contains("HT")) hasHT = true;

                ps[i - 3] = (bid, hasHT, hasDT);
            }
            var t = Task.Run(() =>
            {
                qq.SendGroupMessageAsync(DebugGroup, "嗯");
                var beatmaps = GetBeatmaps(ps.Select(b => b.bid));
                qq.SendGroupMessageAsync(DebugGroup, "好");
                using (var stringWriter = new System.IO.StringWriter())
                {
                    bool needNewLine = false;
                    foreach (var beatmap in beatmaps)
                    {
                        if (needNewLine) stringWriter.Write("\r\n\r\n");

                        if (beatmap.HitLength < 150 ||
                            ps.Any(b => b.hasDT && b.bid == beatmap.Id) && beatmap.HitLength < 200
                            )
                        {
                            qq.SendMessageAsync(endPoint, "!banmap " + beatmap.Id);
                            if (isPrivate) qq.SendGroupMessageAsync(DebugGroup, "!banmap " + beatmap.Id);
                        }
                        stringWriter.WriteLine($"{beatmap.Artist} - {beatmap.Title}[{beatmap.DifficultyName}]");
                        stringWriter.WriteLine($"https://osu.ppy.sh/b/{beatmap.Id}");
                        stringWriter.WriteLine($"Beatmap by {beatmap.Creator}");
                        stringWriter.Write($"{beatmap.TotalLength}s");

                        needNewLine = true;
                    }
                    string m = stringWriter.ToString();
                    qq.SendMessageAsync(endPoint, m, true);
                    if (isPrivate) qq.SendGroupMessageAsync(DebugGroup, m, true);
                }
            });
            Task.Run(() =>
            {
                try
                {
                    t.Wait();
                }
                catch (AggregateException e)
                {
                    qq.SendGroupMessageAsync(DebugGroup, e.InnerException.ToString(), true);
                }
            });
            return true;
        }

        private IEnumerable<Bleatingsheep.OsuMixedApi.Beatmap> GetBeatmaps(IEnumerable<int> bids)
        {
            string key = OsuQqBot.osuApiKey;
            var api = Bleatingsheep.OsuMixedApi.OsuApiClient.ClientUsingKey(key);
            foreach (int bid in bids)
            {
                var beatmaps = api.GetBeatmapsAsync(bid).Result;
                if (beatmaps?.Length >= 1)
                {
                    foreach (var b in beatmaps)
                    {
                        if (b.Id == bid)
                            yield return b;
                    }
                }
            }
        }
    }
}
