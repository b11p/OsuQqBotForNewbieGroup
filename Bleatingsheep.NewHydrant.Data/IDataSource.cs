using Bleatingsheep.OsuMixedApi;

namespace Bleatingsheep.NewHydrant.Data
{
    public interface IDataSource
    {
        OsuApiClient OsuApi { get; }
    }
}
