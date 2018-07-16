using Bleatingsheep.OsuMixedApi;
using Bleatingsheep.OsuMixedApi.MotherShip;
using Bleatingsheep.OsuQqBot.Database.Execution;

namespace Bleatingsheep.NewHydrant.Data
{
    public interface IDataSource
    {
        INewbieDatabase Database { get; }
        OsuApiClient OsuApi { get; }
        MotherShipApiClient MotherShipApi { get; }
    }
}
