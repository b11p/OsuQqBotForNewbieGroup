using Bleatingsheep.NewHydrant.Data;
using Bleatingsheep.NewHydrant.Logging;
using Bleatingsheep.OsuMixedApi;
using Bleatingsheep.OsuMixedApi.MotherShip;
using Bleatingsheep.OsuQqBot.Database.Execution;
using Sisters.WudiLib;

namespace Bleatingsheep.NewHydrant.Core
{
    internal sealed class ExecutingInfo : IDataSource
    {
        public HttpApiClient Qq { get; set; }
        public INewbieDatabase Database { get; set; }
        public OsuApiClient OsuApi { get; set; }
        public MotherShipApiClient MotherShipApi { get; set; }
        public ILogger Logger { get; set; }
        public IDataProvider Data { get; set; }
    }
}
