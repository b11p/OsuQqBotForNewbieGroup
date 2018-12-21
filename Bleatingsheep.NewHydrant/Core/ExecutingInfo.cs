using Bleatingsheep.NewHydrant.Data;
using Bleatingsheep.NewHydrant.Logging;
using Bleatingsheep.OsuMixedApi.MotherShip;

namespace Bleatingsheep.NewHydrant.Core
{
    public sealed class ExecutingInfo
    {
        public ILogger Logger { get; set; }
        public IDataProvider Data { get; set; }
    }
}
