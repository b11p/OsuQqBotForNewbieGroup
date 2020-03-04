using System;
using System.Threading.Tasks;
using Bleatingsheep.OsuMixedApi;
using Bleatingsheep.OsuMixedApi.MotherShip;
using Bleatingsheep.OsuQqBot.Database.Execution;
using Bleatingsheep.OsuQqBot.Database.Models;

namespace Bleatingsheep.NewHydrant.Data
{
    public class DataProvider : IDataProvider
    {
        private readonly OsuApiClient _api;
        private readonly MotherShipApiClient _motherShipApi = new MotherShipApiClient(MotherShipApiClient.DefaultHost);
        private readonly NewbieDatabase _database = new NewbieDatabase();
        
        public DataProvider(OsuApiClient api) => _api = api;

        public async Task<(bool success, BindingInfo result)> GetBindingInfoAsync(long qq)
        {
            var exec = await _database.GetBindingInfoAsync(qq);
            if (!exec.Success)
            {
                OnException?.Invoke(exec.Exception);
                return (false, default(BindingInfo));
            }
            var dbResult = exec.Result;
            if (dbResult != null)
                return (true, dbResult);

            // database no data
            int? idFromMotherShip;
            try
            {
                idFromMotherShip = await _motherShipApi.GetUserBindAsync(qq);
                if (idFromMotherShip == null)
                    return (true, default(BindingInfo));
            }
            catch (OsuApiFailedException)
            {
                // ignore
                return (true, default(BindingInfo));
            }

            // has mother ship result
            var (success, userInfo) = await _api.GetUserInfoAsync(idFromMotherShip.Value, Mode.Standard);
            if (!success)
                return (false, default(BindingInfo)); // osu! api fail
            var username = userInfo?.Name; // 
            if (string.IsNullOrEmpty(username))
                return (true, null);
            var bindExec = await _database.AddNewBindAsync(qq, idFromMotherShip.Value, username, "Mother Ship", null, null);
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
