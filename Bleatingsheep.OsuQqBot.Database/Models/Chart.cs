using Bleatingsheep.OsuMixedApi;
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
        public List<ChartAdministrator> ChartAdministrators { get; set; } = new List<ChartAdministrator>();

        public List<ChartBeatmap> Maps { get; set; } = new List<ChartBeatmap>();
    }

    public class ChartValidGroup
    {
        public int ChartId { get; set; }
        public long GroupId { get; set; }
    }

    public class ChartAdministrator
    {
        public int ChartId { get; set; }
        public long Administrator { get; set; }
    }

    public class ChartBeatmap
    {
        public int BeatmapId { get; set; }
        public Mode Mode { get; set; }
        public string ScoreCalculation { get; set; }
        /// <summary>
        /// 必须全部开启这些 Mod。
        /// </summary>
        public Mods RequiredMods { get; set; }
        /// <summary>
        /// 至少开启其中一个 Mod。
        /// </summary>
        public Mods ForceMods { get; set; }
        public Mods BannedMods { get; set; }

        public List<ChartCommit> Commits { get; set; }

        public int ChartId { get; set; }
    }

    public class ChartCommit
    {
        public int Id { get; set; }

        public int ChartId { get; internal set; }
        public int BeatmapId { get; set; }
        public int Uid { get; set; }

        public DateTimeOffset Date { get; set; }
        public double PPWhenCommit { get; set; }
        public Mods Mods { get; set; }
        public int Combo { get; set; }
        public long Score { get; set; }
        public double Accuracy { get; set; }
        [Required]
        public string Rank { get; set; }
    }
}
