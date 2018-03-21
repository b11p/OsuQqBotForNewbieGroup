using Bleatingsheep.OsuMixedApi;

namespace OsuQqBot.LocalData.DataTypes
{
    /// <summary>
    /// 表示一次 Chart 活动中的单张图（一行）。
    /// </summary>
    class ChartMap
    {
        public int BeatmapId { get; set; }

        public string ScoreCalculation { get; set; }

        public ChartMap(int bid) => BeatmapId = bid;

        Mods RequiredMods { get; set; }

        Mods BannedMods { get; set; }
    }
}
