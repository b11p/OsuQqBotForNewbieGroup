using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sisters.WudiLib;

namespace Bleatingsheep.NewHydrant.Osu.Newbie
{

    public partial class LevelInfo
    {
        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("level_speed")]
        public double LevelSpeed { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("user_id")]
        public long UserId { get; set; }

        [JsonProperty("vip_growth_speed")]
        public int VipGrowthSpeed { get; set; }

        [JsonProperty("vip_growth_total")]
        public int VipGrowthTotal { get; set; }

        [JsonProperty("vip_level")]
        public string VipLevel { get; set; }
    }

    internal static class CqHttpApiExtensions
    {
        public static async Task<LevelInfo> GetLevelInfo(this HttpApiClient api, long qq)
        {
            try
            {
                var levelInfo = await api.CallAsync<LevelInfo>("_get_vip_info", new { user_id = qq });
                return levelInfo;
            }
            catch (ApiAccessException aae)
            when (aae.InnerException is JsonSerializationException e
            && e.InnerException is InvalidCastException)
            {
                return null;
            }
        }
    }
}
