using Newtonsoft.Json;

namespace Bleatingsheep.OsuMixedApi
{
    public class BestPerformance : PlayRecord
    {
        [JsonProperty("pp")]
        public double PP { get; private set; }
    }
}
