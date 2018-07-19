using System;
using Bleatingsheep.Osu.PerformancePlus;

namespace Bleatingsheep.OsuQqBot.Database.Models
{
    public class PlusHistory : UserPlus
    {
        private PlusHistory()
        {
        }

        internal PlusHistory(UserPlus history)
        {
            Id = history.Id;
            Name = history.Name;
            Performance = history.Performance;
            AimTotal = history.AimTotal;
            AimJump = history.AimJump;
            AimFlow = history.AimFlow;
            Precision = history.Precision;
            Speed = history.Speed;
            Stamina = history.Stamina;
            Accuracy = history.Accuracy;
        }

        public DateTimeOffset Date { get; private set; } = DateTimeOffset.UtcNow;
    }
}
