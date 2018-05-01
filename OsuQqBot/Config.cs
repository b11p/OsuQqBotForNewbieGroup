using System;
using System.Collections.Generic;
using System.Text;

namespace OsuQqBot
{
    sealed class Config
    {
        public long Kamisama { get; set; }
        public long MainGroup { get; set; }
        public long[] ValidGroups { get; set; }
        public string ApiKey { get; set; }
        public long Daloubot { get; set; }
    }
}
