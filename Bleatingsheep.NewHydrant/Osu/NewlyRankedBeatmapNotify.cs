using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core;
using Bleatingsheep.OsuMixedApi;
using Sisters.WudiLib;

namespace Bleatingsheep.NewHydrant.Osu
{
    [Function("newly_ranked")]
    internal class NewlyRankedBeatmapNotify : IRegularAsync
    {
        public TimeSpan? OnUtc => null;
        public TimeSpan? Every { get; } = new TimeSpan(0, 15, 0);

        private static IEnumerable<long> s_groups = new List<long>
        {
            72318078,
            672076603,
        };

        private readonly object _thisLock = new object();
        private IEnumerable<int> _oldSets;
        private DateTimeOffset _noMapAfter = DateTimeOffset.MinValue;

        public async Task RunAsync(HttpApiClient api, ExecutingInfo executingInfo)
        {
            IEnumerable<BloodcatBeatmapSet> newSets = null;
            var result = await BloodcatApi.Client.SearchRankedByKeywordAsync();
            lock (_thisLock)
            {
                if (!(_oldSets is null))
                {
                    newSets = result.Take(_oldSets.Count()).Concat(result.Where(s => s.ApprovedDateOffset > _noMapAfter))
                        .SkipWhile(s => _oldSets.Contains(s.Id))
                        .Distinct()
                        .ToList();// 避免延迟求值
                }
                _oldSets = result.Select(s => s.Id);
                if (_noMapAfter < result.Max(s => s.ApprovedDateOffset))
                {
                    _noMapAfter = result.Max(s => s.ApprovedDateOffset);
                }
            }
            var osu = executingInfo.OsuApi;
            if (newSets != null)
            {
                var qq = executingInfo.Qq;
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
                    foreach (long group in s_groups)
                    {
                        await qq.SendGroupMessageAsync(group, message);
                    }
                }
            }
        }
    }
}
