using System;
using System.Collections.Generic;
using System.Text;

namespace OsuQqBot.LocalData.DataTypes
{
    /// <summary>
    /// 表示一次 Chart 活动的类
    /// </summary>
    class Chart
    {
        public int ChartID { get; set; }

        public string Title { get; set; }

        public IList<long> Maps { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTIme { get; set; }

        public double RecommendPerformance { get; set; }

        public double MaximumPerformance { get; set; }
    }
}
