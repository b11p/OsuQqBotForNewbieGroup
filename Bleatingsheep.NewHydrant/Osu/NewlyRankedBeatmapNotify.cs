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
        private DateTimeOffset _noBeatmapAfter = DateTimeOffset.MinValue;

        public async Task RunAsync(HttpApiClient api, ExecutingInfo executingInfo)
        {
            IEnumerable<BloodcatBeatmapSet> newSets = null;
            var result = await BloodcatApi.Client.SearchRankedByKeywordAsync();
            lock (_thisLock)
            {
                var logger = executingInfo.Logger;
                logger.LogInBackground($"取到 {result.Count()} 个");
                logger.LogInBackground($"日期晚于最新：{result.Where(s => s.ApprovedDateOffset > _noBeatmapAfter).Count()} 个");
                if (!(_oldSets is null))
                {
                    logger.LogInBackground($"提取前 {_oldSets.Count()} 个");
                    newSets = result.Take(_oldSets.Count()).Concat(result.Where(s => s.ApprovedDateOffset > _noBeatmapAfter))
                        .Where(s => !_oldSets.Contains(s.Id))
                        .Distinct()
                        .ToList();// 避免延迟求值
                    logger.LogInBackground($"发送 {newSets.Count()} 个");
                }
                _oldSets = result.Select(s => s.Id).ToHashSet();
                if (_noBeatmapAfter < result.Max(s => s.ApprovedDateOffset))
                {
                    _noBeatmapAfter = result.Max(s => s.ApprovedDateOffset);
                    logger.LogInBackground($"日期更新为 {_noBeatmapAfter}");
                }
            }
            var osu = executingInfo.OsuApi;
            if (newSets != null)
            {
                var qq = executingInfo.Qq;
                foreach (var set in newSets)
                {
                    Message message = BloodcatUtilities.GetMusicMessage(osu, set);
                    foreach (long group in s_groups)
                    {
                        await qq.SendGroupMessageAsync(group, message);
                    }
                }
            }
        }
    }
}
