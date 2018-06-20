using System;
using System.Collections.Generic;
using Bleatingsheep.OsuMixedApi;

namespace OsuQqBot.Charts
{
    internal sealed class ChartBeatmapInfo
    {
        private static readonly IReadOnlyDictionary<string, Func<ChartBeatmapInfo, ChartInfoField>> s_ops;

        static ChartBeatmapInfo()
        {
            var dictionary = new Dictionary<string, Func<ChartBeatmapInfo, ChartInfoField>>();
            dictionary.Add("FAIL", bi => bi.AllowFail);
            dictionary.Add("SCORE", bi => bi.Score);
        }

        public int Bid { get; set; }
        public Mode Mode { get; set; }

        public readonly FailField AllowFail = new FailField();
        public readonly ScoreStatisticsField Score = new ScoreStatisticsField();

        internal bool SetInfo(string field, string value)
        {
            return s_ops[field](this).TrySet(value);
        }

        internal bool CancelInfo(string field, string value)
        {
            return s_ops[field](this).TryCancel(value);
        }
    }
}
