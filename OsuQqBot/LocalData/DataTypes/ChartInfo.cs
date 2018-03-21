using System;
using System.Collections.Generic;

namespace OsuQqBot.LocalData.DataTypes
{
    /// <summary>
    /// Chart 活动的额外信息。
    /// </summary>
    class ChartInfo
    {
        public ChartInfo() => StartTime = DateTimeOffset.UtcNow;

        /// <summary>
        /// 如果为<c>true</c>，此 Chart 可以被所有人看到，并且可以复制。
        /// </summary>
        public bool Public { get; set; }

        public DateTimeOffset StartTime { get; set; }

        public DateTimeOffset? EndTime { get; set; }

        public bool IsRunning { get; set; }

        public List<long> Groups { get; set; } = new List<long>();

        public double RecommendPerformance { get; set; }

        public double? MaximumPerformance { get; set; }

        /// <summary>
        /// 在 Fail 的情况下是否可以提交。
        /// </summary>
        public bool AllowsFail { get; set; }
    }
}
