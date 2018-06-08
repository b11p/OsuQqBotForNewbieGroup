using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Bleatingsheep.OsuQqBot.Database.Models
{
    public class Chart
    {
        public int ChartId { get; internal set; }
        [Required]
        public string ChartName { get; set; }
        [Required]
        public string ChartDescription { get; set; }
        public bool Public { get; set; }
        public DateTimeOffset StartTime { get; set; } = DateTimeOffset.Now;
        public DateTimeOffset? EndTime { get; set; }
        public bool IsRunning { get; set; }
        public List<ChartValidGroup> Groups { get; set; } = new List<ChartValidGroup>();
        public double RecommendPerformance { get; set; }
        public double? MaximumPerformance { get; set; }
        public long ChartCreator { get; set; }
        public List<ChartAdministrator> Administrators { get; set; } = new List<ChartAdministrator>();

        public List<ChartBeatmap> Maps { get; set; } = new List<ChartBeatmap>();
    }

    public class ChartValidGroup
    {
        public int ChartId { get; set; }
        public long GroupId { get; set; }

        public static implicit operator ChartValidGroup(long groupId)
            => new ChartValidGroup { GroupId = groupId };
    }

    public class ChartAdministrator
    {
        public int ChartId { get; set; }
        public long Administrator { get; set; }

        public static implicit operator ChartAdministrator(long userId)
            => new ChartAdministrator { Administrator = userId };
    }
}
