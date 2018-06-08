using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bleatingsheep.OsuMixedApi.MotherShip
{
    public class MotherShipApiClient
    {
        public const string DefaultHost = "http://www.mothership.top:8080/";
        public const string BleatingsheepCdnHost = "https://mothership.bleatingsheep.org/";
        private readonly string _host;

        public MotherShipApiClient(string host)
        {
            if (!host?.EndsWith('/') ?? throw new ArgumentNullException(nameof(host))) host += '/';
            _host = host;
        }

        private string UserInfoUrl(long qqId) => _host + $"api/v1/user/qq/{qqId}";

        public async Task<MotherShipResponse<MotherShipUserInfo>> GetUserInfoAsync(long qqId)
        {
            var (success, result) = await Execute.Do(async () =>
            {
                return await HttpMethods.GetJsonDeserializeAsync<MotherShipResponse<MotherShipUserInfo>>(UserInfoUrl(qqId));
            }, "Network error.");
            if (!success) throw new OsuApiFailedException("Network error.");
            return result;
        }
    }
}
