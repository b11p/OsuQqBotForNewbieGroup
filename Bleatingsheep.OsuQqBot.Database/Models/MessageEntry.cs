using System;
using System.ComponentModel.DataAnnotations;

namespace Bleatingsheep.OsuQqBot.Database.Models
{
    public class MessageEntry
    {
        public virtual int Id { get; set; }
        public virtual DateTimeOffset Date { get; set; }
        public virtual long GroupId { get; set; }
        public virtual long UserId { get; set; }
        [Required]
        public virtual string Raw { get; set; }
        [Required]
        public virtual string Text { get; set; }
    }
}
