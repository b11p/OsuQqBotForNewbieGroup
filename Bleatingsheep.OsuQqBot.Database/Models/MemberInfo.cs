using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bleatingsheep.OsuQqBot.Database.Models
{
    public class MemberInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UserId { get; internal set; }

        public bool HadBeenWelcome { get; internal set; }

        public virtual ICollection<GroupMemberInfo> Groups { get; internal set; }
    }
}
