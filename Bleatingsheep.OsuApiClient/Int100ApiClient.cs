using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Bleatingsheep.OsuMixedApi
{
    public class Int100ApiClient
    {
        public static Int100ApiClient ClientUsingKey(string key) => new Int100ApiClient(key);

        private const string apiKeyParameterName = "k";
        private const string qqParameterName = "qq";
        private const string OsuIdParameterName = "u";
        private const string BindIdUrl = "http://www.int100.org/api/bound_qq.php";
        private readonly string key;

        private Int100ApiClient(string key)
        {
            this.key = key;
        }

        public async Task<long?> GetBindedUidAsync(long qq)
        {
            const string bindedUrl = "http://www.int100.org/api/get_id.php";

            var (success, result) = await HttpMethods.GetJsonDeserializeAsync<dynamic>(bindedUrl, (apiKeyParameterName, key), (qqParameterName, qq.ToString()));

            if (!success) return null;

            try
            {
                return result.uid;
            }
            catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
            {
                return null;
            }
        }

        /// <summary>
        /// Bind osu! ID to QQ ID.
        /// </summary>
        /// <param name="qq">QQ ID.</param>
        /// <param name="osuId">Osu! ID.</param>
        /// <returns><c>true</c> if success. Otherwise, false.</returns>
        public async Task<bool> BindQqAndOsuAsync(long qq, int osuId)
        {
            var (success, result) = await HttpMethods.GetJsonDeserializeAsync<Int100ApiBindResult>(BindIdUrl,
                (apiKeyParameterName, key),
                (qqParameterName, qq.ToString()),
                (OsuIdParameterName, osuId.ToString()));
            if (!success) return false;
            return result.Success;
        }

        private sealed class Int100ApiBindResult
        {
            private const int SuccessCode = 500;
            private const string SuccessString = "SUCCESS";

            [JsonProperty("code")]
            public int Code { get; private set; }
            [JsonProperty("msg")]
            public string Message { get; private set; }

            [JsonIgnore]
            public bool Success => Code == SuccessCode && SuccessString == Message;
        }
    }
}
