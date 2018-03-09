using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bleatingsheep.OsuMixedApi
{
    public class OsuApiClient
    {
        #region Addresses
        private static readonly string Root = "https://osu.ppy.sh";
        private static string BeatmapUrl => Root + "/api/get_beatmaps";
        #endregion

        #region Limits
        private static readonly Dictionary<string, OsuApiClient> clients = new Dictionary<string, OsuApiClient>();
        public static OsuApiClient ClientUsingKey(string apiKey)
        {
            lock (clients)
            {
                if (clients.TryGetValue(apiKey, out OsuApiClient client))
                    return client;
                client = new OsuApiClient(apiKey);
                clients.Add(apiKey, client);
                return client;
            }
        }
        private static readonly int span = 60;
        /// <summary>
        /// 每 <see cref="span"/> 秒访问数限制。
        /// </summary>
        public int Limit { get; set; } = 60;
        private readonly Queue<long> apiCalled = new Queue<long>();
        #endregion

        #region ctor and readonly vars
        private readonly string apiKey;
        private OsuApiClient(string apiKey)
        {
            this.apiKey = apiKey;
        }
        #endregion

        #region API
        /// <summary>
        /// 通过 BID 查找图。
        /// </summary>
        /// <param name="bid"></param>
        /// <returns></returns>
        public async Task<Beatmap[]> GetBeatmapsAsync(int bid)
        {
            var result = await SafeGetArrayAsync<Beatmap>(BeatmapUrl, ("k", apiKey), ("b", bid.ToString()));
            return result;
        }
        #endregion

        #region Utils
        private async Task<T[]> SafeGetArrayAsync<T>(string url, params (string key, string value)[] ps)
        {
            // TODO: 增加访问数限制。

            var result = await Utils.GetJsonArrayDeserializeAsync<T>(url, ps);
            return result;
        }
        #endregion
    }
}
