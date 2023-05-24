using System.Threading;
using System.Threading.Tasks;
using Bleatingsheep.Osu;
using Bleatingsheep.Osu.ApiClient;
using Bleatingsheep.OsuQqBot.Database.Models;

namespace Bleatingsheep.NewHydrant.Data
{
    /// <summary>
    /// Not thread-safe.
    /// </summary>
    public interface IDataProvider
    {
        Task<UserBest[]> GetUserBestRetryAsync(int userId, Mode mode, CancellationToken cancellationToken = default);
        
        Task<UserBest[]> GetUserBestRetryLimitAsync(int userId, Mode mode,int limit, CancellationToken cancellationToken = default);

        Task<UserInfo> GetUserInfoRetryAsync(int userId, Mode mode, CancellationToken cancellationToken = default);

        ValueTask<BindingInfo?> GetBindingInfoAsync(long qq);

        Task<int?> GetOsuIdAsync(long qq);

        ValueTask<BeatmapInfo?> GetBeatmapInfoAsync(int beatmapId, Mode mode, CancellationToken cancellationToken = default);
    }
}
