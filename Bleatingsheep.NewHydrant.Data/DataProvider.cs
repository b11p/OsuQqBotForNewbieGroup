using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Bleatingsheep.Osu;
using Bleatingsheep.Osu.ApiClient;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace Bleatingsheep.NewHydrant.Data
{
    public class DataProvider : IDataProvider, IDisposable, ILegacyDataProvider
    {
        private readonly NewbieContext _dbContext;
        private readonly IOsuApiClient _osuApiClient;
        private readonly ThreadLocal<Random> _randomLocal = new(() => new Random());

        public DataProvider(NewbieContext newbieContext, IOsuApiClient osuApiClient)
        {
            _dbContext = newbieContext;
            _osuApiClient = osuApiClient;
        }

        public async Task<(bool success, BindingInfo? result)> GetBindingInfoAsync(long qq)
        {
            try
            {
                var result = await _dbContext.Bindings.SingleOrDefaultAsync(b => b.UserId == qq).ConfigureAwait(false);
                return (true, result);
            }
            catch (Exception e)
            {
                OnException?.Invoke(e);
                return (false, default);
            }
        }

        public async Task<(bool success, int? result)> GetBindingIdAsync(long qq)
        {
            var (success, bi) = await GetBindingInfoAsync(qq).ConfigureAwait(false);
            return (success, bi?.OsuId);
        }

        /// <summary>
        /// 只在调用 <see cref="GetBindingIdAsync(long)"/> 和
        /// <see cref="GetBindingInfoAsync(long)"/> 时可能触发。
        /// </summary>
        public event Action<Exception>? OnException;

        public Task<UserBest[]> GetUserBestRetryAsync(int userId, Mode mode, CancellationToken cancellationToken = default)
        {
            var policy = Policy.Handle<Exception>(e => e is not WebApiClient.HttpStatusFailureException f || f.StatusCode == HttpStatusCode.TooManyRequests)
                .WaitAndRetryForeverAsync(i => TimeSpan.FromMilliseconds((25 << i) + _randomLocal.Value.Next(50)));
            return policy.ExecuteAsync(_ => _osuApiClient.GetUserBest(userId, mode, 100), cancellationToken);
        }

        public void Dispose()
        {
            (_randomLocal as IDisposable)?.Dispose();
        }
    }
}
