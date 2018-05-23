using Bleatingsheep.OsuMixedApi;
using System.Collections.Generic;

namespace Bleatingsheep.OsuQqBot.Database.Models
{
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

        public List<ChartTry> Commits { get; set; }

        public int ChartId { get; set; }

        public Chart Chart { get; set; }

        public static ChartBeatmap FromBid(int bid, Mode mode = Mode.Standard)
            => new ChartBeatmap { BeatmapId = bid, Mode = mode };
    }
}
