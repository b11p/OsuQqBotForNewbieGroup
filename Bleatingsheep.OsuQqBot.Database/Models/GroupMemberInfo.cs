using System.ComponentModel.DataAnnotations.Schema;

namespace Bleatingsheep.OsuQqBot.Database.Models
{
    public class GroupMemberInfo
    {
        public string GroupName { get; set; }
        public virtual MemberGroup Group { get; set; }

        public long UserId { get; set; }
        public virtual MemberInfo Member { get; set; }
    }
}
