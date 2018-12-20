using Bleatingsheep.NewHydrant.Data;
using Bleatingsheep.NewHydrant.Logging;
using Bleatingsheep.OsuMixedApi;
using Bleatingsheep.OsuMixedApi.MotherShip;
using Bleatingsheep.OsuQqBot.Database.Execution;

namespace Bleatingsheep.NewHydrant.Core
{
    public sealed class ExecutingInfo : IDataSource
    {
        public OsuApiClient OsuApi { get; set; }
        public MotherShipApiClient MotherShipApi { get; set; }
        public ILogger Logger { get; set; }
        public IDataProvider Data { get; set; }
    }
}
