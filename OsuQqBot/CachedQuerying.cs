using Bleatingsheep.OsuMixedApi;
using Bleatingsheep.OsuQqBot.Database;
using System.Linq;
using System.Threading.Tasks;

namespace OsuQqBot
{
    public static class CachedQuerying
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bid"></param>
        /// <param name="mode"></param>
        /// <param name="apiKey"></param>
        /// <exception cref="System.ArgumentNullException">网络错误。</exception>
        /// <exception cref="System.ArgumentException">API Key 不正确。</exception>
        /// <returns></returns>
        public static async Task<Beatmap> GetBeatmapAsync(int bid, Mode mode, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new System.ArgumentException("API Key 不正确。", nameof(apiKey));
            }

            var map = await NewbieDatabase.GetBeatmapAsync(bid, mode);
            if (map != null) return map;
            map = (await OsuApiClient.ClientUsingKey(apiKey).GetBeatmapsAsync(bid)).SingleOrDefault();
            if (map == null) return null;
            return await NewbieDatabase.CacheBeatmapAsync(map) ?? map;
        }
    }
}
