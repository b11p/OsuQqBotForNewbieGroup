using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core;
using Bleatingsheep.NewHydrant.Data;
using Bleatingsheep.OsuMixedApi;

namespace Bleatingsheep.NewHydrant.Osu
{
    [Function("osu_init")]
    public class OsuFunction : IInitializable
    {
        protected static OsuApiClient Api { get; private set; }

        private static DataProvider s_data;

        public string Name { get; } = "osu";

#pragma warning disable CS1998
        public async Task<bool> InitializeAsync(ExecutingInfo executingInfo)
        {
            Api = executingInfo.OsuApi;
            s_data = new DataProvider(Api);
            return true;
        }
#pragma warning restore CS1998

        /// <exception cref="ExecutingException"></exception>
        protected async Task<int> EnsureGetBindingIdAsync(long qq)
        {
            var (success, result) = await s_data.GetBindingIdAsync(qq);
            ExecutingException.Ensure(success, "哎，获取绑定信息失败了。");
            ExecutingException.Ensure(result != null, "没绑定！");
            return result.Value;
        }

        protected async Task<UserInfo> EnsureGetUserInfo(string name, Mode mode)
        {
            var (success, result) = await Api.GetUserInfoAsync(name, mode);
            ExecutingException.Ensure(success, "网络错误。");
            ExecutingException.Ensure(result != null, "无此用户！");
            return result;
        }
    }
}
