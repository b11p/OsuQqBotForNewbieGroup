using System;
using System.Threading.Tasks;
using Bleatingsheep.OsuMixedApi;

namespace Bleatingsheep.NewHydrant.Data
{
    internal class DataProvider : IDataProvider
    {
        private readonly IDataSource _source;

        public DataProvider(IDataSource dataSource) => _source = dataSource;

        public async Task<(bool success, int? result)> GetBindingIdAsync(long qq)
        {
            var exec = await _source.Database.GetBindingIdAsync(qq);
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
                result = await _source.MotherShipApi.GetUserBindAsync(qq);
                if (result == null) return (true, result);
            }
            catch (OsuApiFailedException)
            {
                // TODO
                return (false, default(int?));
            }

            // has mother ship result
            var (success, userInfo) = await _source.OsuApi.GetUserInfoAsync(result.Value, Mode.Standard);
            if (!success) return (false, default(int?)); // osu! api fail
            var username = userInfo?.Name; // 
            if (string.IsNullOrEmpty(username)) return (true, null);
            var bindExec = await _source.Database.AddNewBindAsync(qq, result.Value, username, "Mother Ship", null, null);
            return (bindExec.Success, result);
        }

        public event Action<Exception> OnException;
    }
}
