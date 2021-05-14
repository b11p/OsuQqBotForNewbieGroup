using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Bleatingsheep.OsuQqBot.Database.Models
{
    [Index(nameof(SelfId), IsUnique = true)]
    public class DuplicateAuthentication
    {
        public int Id { get; set; }
        public long SelfId { get; set; }
        [Required]
        public string AccessToken { get; set; }
    }
}
