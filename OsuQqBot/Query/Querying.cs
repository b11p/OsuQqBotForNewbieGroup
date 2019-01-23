using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Data;
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

        private OsuApiClient Api => OpenApi.Instance.OsuApiClient;
        private readonly Lazy<IDataProvider> _dataProviderLazy;
        private IDataProvider DataProvider => _dataProviderLazy.Value;

        private Querying()
        {
            _dataProviderLazy = new Lazy<IDataProvider>(() =>
            {
                return new DataProvider(Api);
            });
        }

        public IReadOnlyCollection<UserInfo> CheckUsername(IEnumerable<string> possibleUsernames, bool requireAllSucceeded = true)
        {
            var userInfos = new System.Collections.Concurrent.ConcurrentBag<UserInfo>();
            var api = Api;

            var result = Parallel.ForEach(possibleUsernames, (name, state) =>
            {
                if (state.IsStopped)
                    return;
                var (networkSuccess, userInfo) = api.GetUserInfoAsync(name, Mode.Standard).Result;
                if (requireAllSucceeded && !networkSuccess)
                {
                    state.Stop();
                    return;
                }
                if (userInfo == null)
                    return;
                userInfos.Add(userInfo);
            });

            return result.IsCompleted ? userInfos : null;
        }

        /// <summary>
        /// 查找 QQ 号绑定的 osu! 账号。
        /// </summary>
        /// <exception cref="OsuApiFailedException">访问 API 时出现异常。</exception>
        /// <returns>绑定 osu! 账号的 User ID；如果未绑定，则为 <c>null</c>。</returns>
        public Task<int?> GetUserBind(long qqId) => GetUserBind_Db(qqId);

        /// <summary>
        /// 通过 API (https://api.bleatingsheep.org/api/binding/{qqId}) 查找 QQ 号绑定的 osu! 账号。
        /// </summary>
        /// <exception cref="OsuApiFailedException">访问 API 时出现异常。</exception>
        /// <returns>绑定 osu! 账号的 User ID；如果未绑定，则为 <c>null</c>。</returns>
        private async Task<int?> GetUserBind_Api(long qqId)
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
            catch (Exception e)
            {
                throw CreateEx(e);
            }
        }

        private async Task<int?> GetUserBind_Db(long qqId)
        {
            bool success;
            int? result;
            try
            {
                (success, result) = await DataProvider.GetBindingIdAsync(qqId);
            }
            catch (Exception e)
            {
                throw CreateEx(e);
            }
            return success
                ? result
                : throw CreateEx(null);
        }

        private static OsuApiFailedException CreateEx(Exception e) => new OsuApiFailedException("Something was wrong while finding binding.", e);
    }
}
