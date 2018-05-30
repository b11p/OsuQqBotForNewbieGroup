using Bleatingsheep.OsuMixedApi;
using OsuQqBot.AttributedFunctions;
using Sisters.WudiLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OsuQqBot.NewMapWatching
{
    [Function]
    class NewMapWatcher : IRegularly
    {
        private readonly object _lock = new object();

        public TimeSpan? OnUtc => null;

        public TimeSpan? Every => new TimeSpan(0, 10, 0);

        private IEnumerable<int> _oldSets;

        public void Run()
        {
            IEnumerable<BloodcatBeatmapSet> newSets = null;
            var result = BloodcatApi.Client.SearchRankedByKeywordAsync().Result;
            if (result == null)
            {
                Logger.Log("未能从血猫得到新图");
                return;
            }
            Logger.Log($"{result?.Count()}个图");
            lock (_lock)
            {
                if (!(_oldSets is null))
                {
                    newSets = result.SkipWhile(s => _oldSets.Contains(s.Id)).ToList();// 避免延迟求值
                }
                _oldSets = result.Select(s => s.Id);
            }
            Logger.Log($"有{newSets?.Count()}张新图。");
            var osu = OsuApiClient.ClientUsingKey(OsuQqBot.osuApiKey);
            if (!(newSets is null))
            {
                var qq = OsuQqBot.ApiV2;
                foreach (var set in newSets)
                {
                    int setId = set.Id;
                    string info = string.Empty;
                    info += set.Beatmaps.Max(b => b.TotalLength) + "s, ";
                    if (set.Beatmaps.Length > 1) info += $"{set.Beatmaps.Min(b => b.Stars):0.##}* - {set.Beatmaps.Max(b => b.Stars):0.##}*";
                    else info += $"{set.Beatmaps.Single()?.Stars:0.##}*";
                    info += "\r\n" + $"by {set.Creator}";
                    if (!string.IsNullOrEmpty(set.Source)) info += "\r\n" + $"From {set.Source}";
                    Message message = SendingMessage.MusicCustom(osu.PageOfSet(setId), osu.PreviewAudioOf(setId), $"{set.Artist} - {set.Title}", info, osu.ThumbOf(setId));
                    foreach (long group in OsuQqBot.NotifyGroups)
                    {
                        var task = qq.SendGroupMessageAsync(group, message);
                    }
                }
            }
        }
    }
}
