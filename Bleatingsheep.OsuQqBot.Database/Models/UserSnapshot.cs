using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Bleatingsheep.Osu;
using Bleatingsheep.Osu.ApiClient;

namespace Bleatingsheep.OsuQqBot.Database.Models
{
    public class UserSnapshot
    {
        public long Id { get; set; }
        public int UserId { get; set; }
        public Mode Mode { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset Date { get; set; } = DateTimeOffset.UtcNow;
        [Required]
        public UserInfo UserInfo { get; set; }

        public static UserSnapshot Create(int osuId, Mode mode, UserInfo userInfo)
        {
            return new UserSnapshot
            {
                UserId = osuId,
                Mode = mode,
                UserInfo = userInfo,
            };
        }
    }
}
