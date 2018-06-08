using Bleatingsheep.OsuMixedApi;
using System;
using System.ComponentModel.DataAnnotations;

namespace Bleatingsheep.OsuQqBot.Database.Models
{
    public class ChartTry
    {
        //public int Id { get; set; }

        public int ChartId { get; internal set; }
        public int BeatmapId { get; set; }
        public Mode Mode { get; set; }
        public int UserId { get; set; }

        public long Date { get; set; }
        public double PPWhenCommit { get; set; }
        public Mods Mods { get; set; }
        public int Combo { get; set; }
        public long Score { get; set; }
        public double Accuracy { get; set; }
        [Required]
        public string Rank { get; set; }

        public Chart Chart { get; set; }

        public ChartBeatmap Beatmap { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="beatmap"></param>
        /// <param name="record"></param>
        /// <param name="performance"></param>
        /// <exception cref="ArgumentNullException">beatmap 或 record 为 <c>null</c>。</exception>
        /// <exception cref="InvalidOperationException">
        ///     <see cref="ChartBeatmap.BeatmapId"/> 和 <see cref="PlayRecord.BeatmapId"/> 不一致。
        /// </exception>
        /// <returns></returns>
        public static ChartTry FromRecord(ChartBeatmap beatmap, PlayRecord record, double performance)
        {
            if (beatmap == null) throw new ArgumentNullException(nameof(beatmap));
            if (record == null) throw new ArgumentNullException(nameof(record));
            if (beatmap.BeatmapId != record.BeatmapId) throw new InvalidOperationException("beatmap和record的bid不一致");
            return new ChartTry
            {
                ChartId = beatmap.ChartId,
                BeatmapId = record.BeatmapId,
                Mode = beatmap.Mode,
                UserId = record.UserId,
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
