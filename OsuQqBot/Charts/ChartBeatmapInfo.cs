using Bleatingsheep.OsuMixedApi;

namespace OsuQqBot.Charts
{
    sealed class ChartBeatmapInfo
    {
        public int Bid { get; set; }
        public Mode Mode { get; set; }

        public readonly FailField AllowFail = new FailField();
    }
}
