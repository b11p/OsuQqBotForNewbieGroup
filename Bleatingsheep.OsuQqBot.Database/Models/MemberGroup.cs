using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Bleatingsheep.OsuQqBot.Database.Models
{
    public class MemberGroup
    {
        [Key, Required]
        public string Name { get; internal set; }

        public virtual ICollection<GroupMemberInfo> Members { get; internal set; }
    }
}
