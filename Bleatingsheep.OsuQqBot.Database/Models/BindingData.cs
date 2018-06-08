using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bleatingsheep.OsuQqBot.Database.Models
{
    /// <summary>
    /// 一个QQ绑定的osu!账号的数据
    /// </summary>
    public class BindingInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UserId { get; set; }
        public int OsuId { get; set; }
        [Required]
        public string Source { get; set; }
    }
}
