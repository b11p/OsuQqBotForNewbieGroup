using System;
using System.Collections.Generic;
using System.Text;

namespace OsuQqBot.LocalData.DataTypes
{
    class Tip
    {
        public string Content { get; set; }
        public long CreatedByQq { get; set; }
        public string CreatedByName { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
