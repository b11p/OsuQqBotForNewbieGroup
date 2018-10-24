using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Bleatingsheep.OsuMixedApi;
using Newtonsoft.Json;

namespace OsuQqBot.Query
{
    /// <summary>
    /// 利用各种方法拿到想要的数据，并且负责一定的同步的方法。
    /// </summary>
    public sealed class Querying
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
        /// <param name="qqId"></param>
        /// <exception cref="OsuApiFailedException"></exception>
        /// <returns></returns>
        public async Task<int?> GetUserBind(long qqId)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var httpResponse = await httpClient.GetAsync($"https://api.bleatingsheep.org/api/binding/{qqId}");
                    return httpResponse.StatusCode == System.Net.HttpStatusCode.NotFound
                        ? null
                        : (int?)JsonConvert.DeserializeObject<dynamic>(await httpResponse.Content.ReadAsStringAsync()).osuId;
                }
            }
            catch (Exception)
            {
                throw new OsuApiFailedException();
            }
        }
    }
}
