using Bleatingsheep.OsuMixedApi;
using Bleatingsheep.OsuMixedApi.MotherShip;
using Bleatingsheep.OsuQqBot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OsuQqBot.Query
{
    /// <summary>
    /// 利用各种方法拿到想要的数据，并且负责一定的同步的方法。
    /// </summary>
    sealed class Querying
    {
        static Querying()
        {
            var q = new Querying();
            Instance = q;
        }

        public static Querying Instance { get; private set; } = null;

        private readonly OsuApiClient _api = null;

        private Querying() => _api = OpenApi.Instance.OsuApiClient;

        public IReadOnlyCollection<UserInfo> CheckUsername(IEnumerable<string> possibleUsernames, bool requireAllSucceeded = true)
        {
            var userInfos = new System.Collections.Concurrent.ConcurrentBag<UserInfo>();
            var api = _api;

            var result = Parallel.ForEach(possibleUsernames, (name, state) =>
            {
                if (state.IsStopped) return;
                var (networkSuccess, userInfo) = api.GetUserInfoAsync(name, Mode.Standard).Result;
                if (requireAllSucceeded && !networkSuccess)
                {
                    state.Stop();
                    return;
                }
                if (userInfo == null) return;
                userInfos.Add(userInfo);
            });

            if (!result.IsCompleted) return null;
            return userInfos;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bid"></param>
        /// <param name="mode"></param>
        /// <param name="apiKey"></param>
        /// <exception cref="ArgumentException">API Key 不正确。</exception>
        /// <returns></returns>
        public async Task<Beatmap> GetBeatmapAsync(int bid, Mode mode, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException("API Key 不正确。", nameof(apiKey));
            }

            var map = await NewbieDatabase.GetBeatmapAsync(bid, mode);
            if (map != null) return map;
            map = (await _api.GetBeatmapsAsync(bid))?.SingleOrDefault();
            if (map == null) return null;
            if (!map.IsInfoFixed()) return map;
            return await NewbieDatabase.CacheBeatmapAsync(map) ?? map;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="qqId"></param>
        /// <exception cref="OsuApiFailedException"></exception>
        /// <returns></returns>
        public async Task<int?> GetUserBind(long qqId)
        {
            var result = await OpenApi.Instance.Bindings.GetBindingIdAsync(qqId);
            if (result is int u) return u;
            var response = await OpenApi.Instance.MotherShipApiClient.GetUserInfoAsync(qqId);
            if (response.Data is MotherShipUserInfo info)
            {
                u = info.OsuId;
                await OpenApi.Instance.Bindings.BindAsync(qqId, info.OsuId, info.Name, "Mother Ship (while running)", 0, null);
                return u;
            }
            return null;
        }
    }
}
