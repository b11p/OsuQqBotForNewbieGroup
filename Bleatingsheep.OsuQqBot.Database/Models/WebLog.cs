using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bleatingsheep.OsuQqBot.Database.Models
{
    public class WebLog
    {
        public long Id { get; set; }

        public string User { get; set; }

        public string Token { get; set; }

        public DateTimeOffset Time { get; set; } = DateTimeOffset.Now;

        [Required]
        public string IPAddress { get; set; }

        public string Location { get; set; }

        [Required]
        public string Kind { get; set; }
    }
}
