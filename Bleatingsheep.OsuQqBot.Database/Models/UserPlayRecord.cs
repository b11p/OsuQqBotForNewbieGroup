using System.ComponentModel.DataAnnotations;
using Bleatingsheep.Osu.ApiClient;
using Mode = Bleatingsheep.Osu.Mode;

namespace Bleatingsheep.OsuQqBot.Database.Models
{
    public class UserPlayRecord
    {
        public int UserId { get; set; }
        public int PlayNumber { get; set; }
        public Mode Mode { get; set; }
        [Required]
        public UserRecent Record { get; set; }

        public static UserPlayRecord Create(int osuId, Mode mode, int playNumber, UserRecent userRecent)
        {
            return new UserPlayRecord
            {
                UserId = osuId,
                Mode = mode,
                PlayNumber = playNumber,
                Record = userRecent,
            };
        }
    }
}
