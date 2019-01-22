using System;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Core;
using Sisters.WudiLib;

namespace Bleatingsheep.NewHydrant.Attributions
{
    public interface IRegularAsync
    {
        TimeSpan? OnUtc { get; }
        TimeSpan? Every { get; }
        Task RunAsync(HttpApiClient api);
    }
}
