using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Polly;

namespace Bleatingsheep.OsuMixedApi
{
    public class OsuApiClient
    {
        #region Static
        internal static readonly TimeSpan TimeZone = new TimeSpan(8, 0, 0);
        #endregion

        #region Addresses
        private static readonly string Root = "https://osu.ppy.sh";
        private static string BeatmapUrl => Root + "/api/get_beatmaps";
        private static string RecentlyPlayedUrl => Root + "/api/get_user_recent";
        private static string BestPerformanceUrl => Root + "/api/get_user_best";
        private static string UserUrl => Root + "/api/get_user";
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

        public async Task<Beatmap[]> GetBeatmapAsync(byte[] md5)
        {
            string md5String = BitConverter.ToString(md5).Replace("-", string.Empty);
            return await GetBeatmapAsync(md5String);
        }

        public async Task<Beatmap[]> GetBeatmapAsync(string md5) => await SafeGetArrayAsync<Beatmap>(BeatmapUrl, ("k", apiKey), ("h", md5));

        //[Obsolete]
        public async Task<BestPerformance[]> GetBestPerformancesAsync(int uid, Mode mode, int limit = 10)
            => await GetBestPerformancesAsync(uid, (Osu.Mode)mode, limit);

        public async Task<BestPerformance[]> GetBestPerformancesAsync(int uid, Osu.Mode mode, int limit = 10)
        {
            var result = await GetBestPerformancesAsync(uid.ToString(), "u", mode, limit);
            return result;
        }

        //[Obsolete]
        public async Task<BestPerformance[]> GetBestPerformancesAsync(string username, Mode mode, int limit = 10)
            => await GetBestPerformancesAsync(username, (Osu.Mode)mode, limit);

        public async Task<BestPerformance[]> GetBestPerformancesAsync(string username, Osu.Mode mode, int limit = 10)
        {
            var result = await GetBestPerformancesAsync(username, "string", mode, limit);
            return result;
        }

        //[Obsolete]
        public async Task<PlayRecord[]> GetRecentlyAsync(int uid, Mode mode, int limit = 10)
            => await GetRecentlyAsync(uid, (Osu.Mode)mode, limit);

        public async Task<PlayRecord[]> GetRecentlyAsync(int uid, Osu.Mode mode, int limit = 10)
        {
            var result = await GetRecentlyAsync(uid.ToString(), "u", mode, limit);
            return result;
        }

        //[Obsolete]
        public async Task<PlayRecord[]> GetRecentlyAsync(string username, Mode mode, int limit = 10)
            => await GetRecentlyAsync(username, (Osu.Mode)mode, limit);

        public async Task<PlayRecord[]> GetRecentlyAsync(string username, Osu.Mode mode, int limit = 10)
        {
            var result = await GetRecentlyAsync(username, "string", mode, limit);
            return result;
        }

        //[Obsolete]
        public async Task<(bool networkSuccess, UserInfo)> GetUserInfoAsync(int uid, Mode mode)
            => await GetUserInfoAsync(uid, (Osu.Mode)mode);

        public async Task<(bool networkSuccess, UserInfo)> GetUserInfoAsync(int uid, Osu.Mode mode)
        {
            var result = await GetUserInfoAsync(uid.ToString(), "u", mode);
            if (result == null)
                return (false, null);
            var filter = result.Where(u => u.Id == uid);
            return (true, filter.SingleOrDefault());
        }

        //[Obsolete]
        public async Task<(bool networkSuccess, UserInfo)> GetUserInfoAsync(string username, Mode mode)
            => await GetUserInfoAsync(username, (Osu.Mode)mode);

        public async Task<(bool networkSuccess, UserInfo)> GetUserInfoAsync(string username, Osu.Mode mode)
        {
            var result = await GetUserInfoAsync(username, "string", mode);
            if (result == null)
                return (false, null);
            var filter = result.Where(u => string.Equals(u.Name, username, StringComparison.OrdinalIgnoreCase));
            return (true, filter.SingleOrDefault());
        }
        #endregion

        #region Urls
        public string ThumbOf(int setId) => $"https://b.ppy.sh/thumb/{setId}l.jpg";

        public string PreviewAudioOf(int setId) => $"https://b.ppy.sh/preview/{setId}.mp3";

        public string PageOfSet(int setId) => $"https://osu.ppy.sh/beatmapsets/{setId}";

        public string PageOfSetOld(int setId) => $"https://osu.ppy.sh/s/{setId}";
        #endregion

        #region Utils
        private readonly ThreadSafeRandom _threadSafeRandom = new ThreadSafeRandom();

        private async Task<T[]> SafeGetArrayAsync<T>(string url, params (string key, string value)[] ps)
        {
            // TODO: 增加访问数限制。

            // Exponential backoff 指数退避
            return await Policy
                .HandleResult((T[])null)
                .WaitAndRetryAsync(2, t => TimeSpan.FromMilliseconds(_threadSafeRandom.Next(75 << (t - 1)) + 1))
                .ExecuteAsync(() => HttpMethods.GetJsonArrayDeserializeAsync<T>(url, ps))
                .ConfigureAwait(false);
        }

        private async Task<PlayRecord[]> GetRecentlyAsync(string u, string type, Osu.Mode m, int limit)
        {
            var result = await SafeGetArrayAsync<PlayRecord>(RecentlyPlayedUrl,
                ("k", apiKey),
                ("u", u),
                ("m", ((int)m).ToString()),
                ("limit", limit.ToString()),
                ("type", type));
            if (result != null)
                Array.ForEach(result, recent => recent.Mode = (Mode)m);
            return result;
        }

        private async Task<BestPerformance[]> GetBestPerformancesAsync(string u, string type, Osu.Mode m, int limit)
        {
            var result = await SafeGetArrayAsync<BestPerformance>(BestPerformanceUrl,
               ("k", apiKey),
               ("u", u),
               ("m", ((int)m).ToString()),
               ("limit", limit.ToString()),
               ("type", type));
            if (result != null)
                Array.ForEach(result, bp => bp.Mode = (Mode)m);
            return result;
        }

        private async Task<UserInfo[]> GetUserInfoAsync(string u, string type, Osu.Mode m, int event_days = 1)
        {
            var result = await SafeGetArrayAsync<UserInfo>(UserUrl,
                ("k", apiKey),
                ("u", u),
                ("m", ((int)m).ToString()),
                ("event_days", event_days.ToString()),
                ("type", type));
            if (result != null)
                Array.ForEach(result, user => user.Mode = (Mode)m);
            return result;
        }
        #endregion
    }
}
