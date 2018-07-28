using System;
using System.Threading.Tasks;
using Bleatingsheep.OsuMixedApi;
using Bleatingsheep.OsuQqBot.Database.Execution;
using Bleatingsheep.OsuQqBot.Database.Models;

namespace Bleatingsheep.NewHydrant.Data
{
    internal class DataProvider : IDataProvider
    {
        private readonly IDataSource _source;

        public DataProvider(IDataSource dataSource) => _source = dataSource;

        public async Task<(bool success, BindingInfo result)> GetBindingInfoAsync(long qq)
        {
            var exec = await _source.Database.GetBindingInfoAsync(qq);
            if (!exec.Success)
            {
                OnException?.Invoke(exec.Exception);
                return (false, default(BindingInfo));
            }
            var dbResult = exec.Result;
            if (dbResult != null) return (true, dbResult);

            // database no data
            int? idFromMotherShip;
            try
            {
                idFromMotherShip = await _source.MotherShipApi.GetUserBindAsync(qq);
                if (idFromMotherShip == null) return (true, default(BindingInfo));
            }
            catch (OsuApiFailedException)
            {
                // TODO
                return (false, default(BindingInfo));
            }

            // has mother ship result
            var (success, userInfo) = await _source.OsuApi.GetUserInfoAsync(idFromMotherShip.Value, Mode.Standard);
            if (!success) return (false, default(BindingInfo)); // osu! api fail
            var username = userInfo?.Name; // 
            if (string.IsNullOrEmpty(username)) return (true, null);
            var bindExec = await _source.Database.AddNewBindAsync(qq, idFromMotherShip.Value, username, "Mother Ship", null, null);
            return (bindExec.TryGetResult(out var bindResult), bindResult);
        }

        public async Task<(bool success, int? result)> GetBindingIdAsync(long qq)
        {
            var (success, bi) = await GetBindingInfoAsync(qq);
            return (success, bi?.OsuId);
        }

        public event Action<Exception> OnException;
    }
}
