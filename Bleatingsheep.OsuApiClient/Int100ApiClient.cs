using System.Threading.Tasks;

namespace Bleatingsheep.OsuMixedApi
{
    public class Int100ApiClient
    {
        public static Int100ApiClient ClientUsingKey(string key) => new Int100ApiClient(key);

        private const string apiKeyParameterName = "k";
        private const string qqParameterName = "qq";
        private readonly string key;

        private Int100ApiClient(string key)
        {
            this.key = key;
        }

        public async Task<long?> GetBindedUidAsync(long qq)
        {
            const string bindedUrl = "http://www.int100.org/api/get_id.php";

            var (success, result) = await Utils.GetJsonDeserializeAsync<dynamic>(bindedUrl, (apiKeyParameterName, key), (qqParameterName, qq.ToString()));

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
    }
}
