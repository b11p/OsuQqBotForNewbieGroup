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
        public Mods BannedMods { get; set; } = Mods.DoubleTime | Mods.HalfTime;
        public bool AllowsFail { get; set; }

        public List<ChartCommit> Commits { get; set; }

        public int ChartId { get; set; }

        public Chart Chart { get; set; }

        public static ChartBeatmap FromBid(int bid, Mode mode = Mode.Standard)
            => new ChartBeatmap { BeatmapId = bid, Mode = mode };
    }

    public class ChartCommit
    {
        //public int Id { get; set; }

        public int ChartId { get; internal set; }
        public int BeatmapId { get; set; }
        public Mode Mode { get; set; }
        public int Uid { get; set; }

        public long Date { get; set; }
        public double PPWhenCommit { get; set; }
        public Mods Mods { get; set; }
        public int Combo { get; set; }
        public long Score { get; set; }
        public double Accuracy { get; set; }
        [Required]
        public string Rank { get; set; }

        public ChartBeatmap Beatmap { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="beatmap"></param>
        /// <param name="record"></param>
        /// <param name="performance"></param>
        /// <exception cref="ArgumentNullException">beatmap 或 record 为 <c>null</c>。</exception>
        /// <exception cref="InvalidOperationException">
        ///     <see cref="ChartBeatmap.BeatmapId"/> 和 <see cref="PlayRecord.Bid"/> 不一致。
        /// </exception>
        /// <returns></returns>
        public static ChartCommit FromRecord(ChartBeatmap beatmap, PlayRecord record, double performance)
        {
            if (beatmap == null) throw new ArgumentNullException(nameof(beatmap));
            if (record == null) throw new ArgumentNullException(nameof(record));
            if (beatmap.BeatmapId != record.Bid) throw new InvalidOperationException("beatmap和record的bid不一致");
            return new ChartCommit
            {
                ChartId = beatmap.ChartId,
                BeatmapId = record.Bid,
                Mode = beatmap.Mode,
                Uid = record.Uid,
                Date = record.DateOffset.ToUnixTimeSeconds(),
                PPWhenCommit = performance,
                Mods = record.Mods,
                Combo = record.Combo,
                Score = record.Score,
                Accuracy = record.Pass ? record.Accuracy : 0,
                Rank = record.Rank,
            };
        }
    }
}
