using System;
using System.ComponentModel.DataAnnotations.Schema;
using Bleatingsheep.Osu;

namespace Bleatingsheep.OsuQqBot.Database.Models
{
    public class PlayRecordQueryTemp
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public Mode Mode { get; set; }
        public int StartNumber { get; set; }
    }
}
