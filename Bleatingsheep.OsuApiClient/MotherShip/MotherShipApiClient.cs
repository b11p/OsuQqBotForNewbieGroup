using System;
using System.Threading.Tasks;

namespace Bleatingsheep.OsuMixedApi.MotherShip
{
    public class MotherShipApiClient
    {
        public const string DefaultHost = "https://www.mothership.top/";
        public const string LegacyInsecureHost = "http://www.mothership.top:8080/";
        private readonly string _host;

        public MotherShipApiClient(string host)
        {
            if (!host?.EndsWith("/", StringComparison.Ordinal) ?? throw new ArgumentNullException(nameof(host)))
                host += '/';
            _host = host;
        }

        private string UserInfoUrl(long qqId) => _host + $"api/v1/user/qq/{qqId}";
        private string UserYesterdayInfoUrl(int osuId) => _host + $"api/v1/userinfo/nearest/{osuId}";

        public string GetStatUrl(int osuId) => _host + $"api/v1/stat/{osuId}";

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="OsuApiFailedException">访问网络失败。</exception>
        /// <param name="qqId"></param>
        /// <returns></returns>
        public virtual async Task<MotherShipResponse<MotherShipUserInfo>> GetUserInfoAsync(long qqId)
        {
            var (success, result) = await Execute.Do(async () =>
            {
                return await HttpMethods.GetJsonDeserializeAsync<MotherShipResponse<MotherShipUserInfo>>(UserInfoUrl(qqId));
            }, "Network error.");
            if (!success)
                throw new OsuApiFailedException("Network error.");
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="osuId"></param>
        /// <exception cref="OsuApiFailedException"></exception>
        /// <returns></returns>
        public virtual async Task<MotherShipResponse<UserHistory>> GetYesterdayInfo(int osuId)
        {
            var (success, result) = await Execute.Do(async () =>
            {
                return await HttpMethods.GetJsonDeserializeAsync<MotherShipResponse<UserHistory>>(UserYesterdayInfoUrl(osuId));
            }, "Network error.");
            if (!success)
                throw new OsuApiFailedException("Network error.");
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="qqId"></param>
        /// <exception cref="OsuApiFailedException">访问网络失败。</exception>
        /// <returns></returns>
        public async Task<int?> GetUserBindAsync(long qqId)
        {
            var response = await GetUserInfoAsync(qqId);
            return response?.IsSuccessStatusCode() == true ? (int?)response.Data.OsuId : null;
        }
    }
}
