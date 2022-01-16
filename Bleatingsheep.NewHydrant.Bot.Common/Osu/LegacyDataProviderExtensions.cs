using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Core;
using Bleatingsheep.NewHydrant.Data;
using Bleatingsheep.OsuMixedApi;
using UserInfo = Bleatingsheep.OsuMixedApi.UserInfo;

namespace Bleatingsheep.NewHydrant.Osu
{
    public static class LegacyDataProviderExtensions
    {
        /// <summary>
        /// 确保。
        /// </summary>
        /// <exception cref="ExecutingException"></exception>
        public static async Task<int> EnsureGetBindingIdAsync(this ILegacyDataProvider dataProvider, long qq)
        {
            var (success, result) = await dataProvider.GetBindingIdAsync(qq);
            ExecutingException.Ensure(success, "哎，获取绑定信息失败了。");
            ExecutingException.Ensure(result != null, "没有绑定 osu! 账号。见https://github.com/bltsheep/OsuQqBotForNewbieGroup/wiki/%E5%B0%86-QQ-%E5%8F%B7%E4%B8%8E-osu!-%E8%B4%A6%E5%8F%B7%E7%BB%91%E5%AE%9A");
            return result.Value;
        }

        public static async Task<UserInfo> EnsureGetUserInfo(this OsuApiClient osuApi, string name, Bleatingsheep.Osu.Mode mode)
        {
            var (success, result) = await osuApi.GetUserInfoAsync(name, mode);
            ExecutingException.Ensure(success, "网络错误。");
            ExecutingException.Ensure(result != null, "无此用户！");
            return result;
        }
    }
}
