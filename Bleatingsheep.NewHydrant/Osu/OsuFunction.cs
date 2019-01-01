using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Core;
using Bleatingsheep.NewHydrant.Data;
using Bleatingsheep.NewHydrant.Logging;
using Bleatingsheep.OsuMixedApi;
using Bleatingsheep.OsuQqBot.Database.Execution;

namespace Bleatingsheep.NewHydrant.Osu
{
    public class OsuFunction
    {
        protected static OsuApiClient OsuApi { get; private set; }

        protected static IDataProvider DataProvider { get; private set; }

        protected static INewbieDatabase Database { get; } = new NewbieDatabase();

        protected static ILogger Logger => FileLogger.Default;

        public static void SetApiKey(string apiKey)
        {
            OsuApi = OsuApiClient.ClientUsingKey(apiKey);
            DataProvider = new DataProvider(OsuApi);
        }

        /// <exception cref="ExecutingException"></exception>
        protected async Task<int> EnsureGetBindingIdAsync(long qq)
        {
            var (success, result) = await DataProvider.GetBindingIdAsync(qq);
            ExecutingException.Ensure(success, "哎，获取绑定信息失败了。");
            ExecutingException.Ensure(result != null, "没有绑定 osu! 账号。见https://github.com/bltsheep/OsuQqBotForNewbieGroup/wiki/%E5%B0%86-QQ-%E5%8F%B7%E4%B8%8E-osu!-%E8%B4%A6%E5%8F%B7%E7%BB%91%E5%AE%9A");
            return result.Value;
        }

        protected async Task<UserInfo> EnsureGetUserInfo(string name, Mode mode)
        {
            var (success, result) = await OsuApi.GetUserInfoAsync(name, mode);
            ExecutingException.Ensure(success, "网络错误。");
            ExecutingException.Ensure(result != null, "无此用户！");
            return result;
        }
    }
}
