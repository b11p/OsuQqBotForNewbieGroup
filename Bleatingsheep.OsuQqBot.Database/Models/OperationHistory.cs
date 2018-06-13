using System;
using System.ComponentModel.DataAnnotations;

namespace Bleatingsheep.OsuQqBot.Database.Models
{
    public class OperationHistory
    {
        private DateTime _date = DateTime.UtcNow;

        public int Id { get; private set; }
        public long UserId { get; set; }
        public string User { get; set; }
        public Operation Operation { get; set; }
        public long? OperatorId { get; set; }
        public string Operator { get; set; }

        public DateTime Date
        {
            get => _date;
            set => _date = value.ToUniversalTime();
        }

        [Required]
        public string Remark { get; set; }
    }

    public enum Operation
    {
        Requesting = 0,
        Joining = 1,
        Binding = 2,
    }
}
