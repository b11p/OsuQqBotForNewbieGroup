using System;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Core;
using Bleatingsheep.NewHydrant.Logging;
using Bleatingsheep.Osu.ApiClient;
using Bleatingsheep.OsuMixedApi;
using Microsoft.Extensions.Caching.Memory;
using UserInfo = Bleatingsheep.OsuMixedApi.UserInfo;

namespace Bleatingsheep.NewHydrant.Osu
{
    public class OsuFunction : Service
    {
        protected static ILogger FLogger => FileLogger.Default;
    }
}
