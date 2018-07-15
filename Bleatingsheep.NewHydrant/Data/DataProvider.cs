using System;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Core;
using Sisters.WudiLib;

namespace Bleatingsheep.NewHydrant.Data
{
    internal class DataProvider : IDataProvider
    {
        private readonly ExecutingInfo _executingInfo;

        public DataProvider(ExecutingInfo executingInfo) => _executingInfo = executingInfo;

        public async Task<(bool success, int? result)> GetBindingIdAsync(long qq)
        {
            var exec = await _executingInfo.Database.GetBindingIdAsync(qq);
            if (!exec.Success)
            {
                OnException?.Invoke(exec.Exception);
                return (false, default(int?));
            }
            var result = exec.Result;
            if (result.HasValue) return (true, result);

            // database no data
            try
            {
                result = await _executingInfo.MotherShipApi.GetUserBindAsync(qq);
                if (result == null) return (true, result);
            }
            catch (ApiAccessException)
            {
                // TODO
                return (false, default(int?));
            }

            // has mother ship result
            var (success, userInfo) = await _executingInfo.OsuApi.GetUserInfoAsync(result.Value, OsuMixedApi.Mode.Standard);
            if (!success) return (false, default(int?)); // osu! api fail
            var username = userInfo?.Name; // 
            if (string.IsNullOrEmpty(username)) return (true, null);
            var bindExec = await _executingInfo.Database.AddNewBindAsync(qq, result.Value, username, "Mother Ship", null, null);
            return (bindExec.Success, result);
        }

        public event Action<Exception> OnException;
    }
}
