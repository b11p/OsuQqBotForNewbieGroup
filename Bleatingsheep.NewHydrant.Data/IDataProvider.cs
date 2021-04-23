using System.Threading;
using System.Threading.Tasks;
using Bleatingsheep.Osu;
using Bleatingsheep.Osu.ApiClient;

namespace Bleatingsheep.NewHydrant.Data
{
    public interface IDataProvider
    {
        Task<UserBest[]> GetUserBestRetryAsync(int userId, Mode mode, CancellationToken cancellationToken = default);
    }
}
