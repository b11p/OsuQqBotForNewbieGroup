using System;
using System.Collections.Generic;
using System.Text;

namespace OsuQqBot.LocalData.DataTypes
{
    class ChartCommit
    {
        public string ChartTitle { get; set; }

        public long Uid { get; set; }

        public double Performance { get; set; }

        public DateTime CommitTime { get; set; }

        public double Accuracy { get; set; }

        public int Combo { get; set; }

        public long Score { get; set; }
    }
}
