using System;
using System.ComponentModel.DataAnnotations;
using Bleatingsheep.Osu;

namespace Bleatingsheep.OsuQqBot.Database.Models
{
    public class UpdateSchedule
    {
        public int UserId { get; set; }
        public Mode Mode { get; set; }
        public DateTimeOffset NextUpdate { get; set; }
        public int ActiveIndex { get; set; }
        [Timestamp]
        public byte[] Version { get; set; }
    }
}
