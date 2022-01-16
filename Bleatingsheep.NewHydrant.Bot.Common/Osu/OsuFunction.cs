using System;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Core;
using Bleatingsheep.NewHydrant.Logging;
using Bleatingsheep.Osu.ApiClient;
using Bleatingsheep.OsuMixedApi;
using Microsoft.Extensions.Caching.Memory;
using UserInfo = Bleatingsheep.OsuMixedApi.UserInfo;

namespace Bleatingsheep.NewHydrant.Osu
{
    public class OsuFunction : Service
    {
        protected static OsuApiClient OsuApi { get; private set; }

        private static WebApiClient.HttpApiFactory<IOsuApiClient> s_osuApiFactory;
        protected static IOsuApiClient CreateOsuApi() => s_osuApiFactory.CreateHttpApi();

        protected static ILogger FLogger => FileLogger.Default;

        public static void SetApiKey(string apiKey)
        {
            OsuApi = OsuApiClient.ClientUsingKey(apiKey);

            s_osuApiFactory = OsuApiClientFactory.CreateFactory(apiKey);
        }

        protected async Task<UserInfo> EnsureGetUserInfo(string name, Bleatingsheep.Osu.Mode mode)
        {
            var (success, result) = await OsuApi.GetUserInfoAsync(name, mode);
            ExecutingException.Ensure(success, "网络错误。");
            ExecutingException.Ensure(result != null, "无此用户！");
            return result;
        }

        private static readonly IMemoryCache s_cache = new MemoryCache(new MemoryCacheOptions());

        private static readonly TimeSpan CacheAvailable = TimeSpan.FromMinutes(10);

        protected async Task<(bool, UserInfo)> GetCachedUserInfo(int id, Bleatingsheep.Osu.Mode mode)
        {
            var hasCache = s_cache.TryGetValue<UserInfo>((id, mode), out var cachedInfo);
            if (hasCache)
            {
                return (true, cachedInfo);
            }
            var (success, userInfo) = await OsuApi.GetUserInfoAsync(id, mode);
            if (success)
            {
                s_cache.Set((id, mode), userInfo, CacheAvailable);
                return (true, userInfo);
            }
            else
                // fail
                return default;
        }
    }
}
