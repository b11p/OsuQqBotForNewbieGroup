using System;
using System.Collections.Generic;
using System.Text;
using Bleatingsheep.Osu;
using Bleatingsheep.Osu.ApiClient;

namespace Bleatingsheep.OsuQqBot.Database.Models
{
    public class UserHistoryInfo : UserInfo
    {
        public Mode Mode { get; set; }
        public DateTimeOffset Date { get; set; } = DateTimeOffset.UtcNow;
    }
}
