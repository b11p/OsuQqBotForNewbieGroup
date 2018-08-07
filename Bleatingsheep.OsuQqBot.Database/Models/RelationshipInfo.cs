using System.ComponentModel.DataAnnotations;

namespace Bleatingsheep.OsuQqBot.Database.Models
{
    public class RelationshipInfo
    {
        public long UserId { get; set; }
        public string Relationship { get; set; }
        [ConcurrencyCheck]
        public int Target { get; set; }
    }
}
