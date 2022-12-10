using System.Threading;
using System.Threading.Tasks;
using Bleatingsheep.Osu;
using Bleatingsheep.Osu.ApiClient;

namespace Bleatingsheep.NewHydrant.Data
{
    /// <summary>
    /// Not thread-safe.
    /// </summary>
    public interface IDataProvider
    {
        Task<UserBest[]> GetUserBestRetryAsync(int userId, Mode mode, CancellationToken cancellationToken = default);

        Task<UserInfo> GetUserInfoRetryAsync(int userId, Mode mode, CancellationToken cancellationToken = default);

        Task<int?> GetOsuIdAsync(long qq);

        ValueTask<BeatmapInfo?> GetBeatmapInfoAsync(int beatmapId, Mode mode, CancellationToken cancellationToken = default);
    }
}
