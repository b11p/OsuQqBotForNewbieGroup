using System;
using System.Diagnostics;
using System.Net;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Bleatingsheep.Osu;
using Bleatingsheep.Osu.ApiClient;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;

namespace Bleatingsheep.NewHydrant.Data
{
    public class DataProvider : IDataProvider, ILegacyDataProvider
    {
        private readonly IDbContextFactory<NewbieContext> _dbContextFactory;
        private readonly IOsuApiClient _osuApiClient;
        private readonly ILogger<DataProvider> _logger;

        public DataProvider(IOsuApiClient osuApiClient, IDbContextFactory<NewbieContext> dbContextFactory, ILogger<DataProvider> logger)
        {
            _osuApiClient = osuApiClient;
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        private async Task<(bool success, BindingInfo? result)> GetBindingInfoLegacyAsync(long qq)
        {
            try
            {
                var result = await GetBindingInfoAsync(qq).ConfigureAwait(false);
                return (true, result);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{message}", e.Message);
                return (false, default);
            }
        }

        public async ValueTask<BindingInfo?> GetBindingInfoAsync(long qq)
        {
            var db = _dbContextFactory.CreateDbContext();
            await using (db.ConfigureAwait(false))
            {
                return await db.Bindings.FirstOrDefaultAsync(b => b.UserId == qq).ConfigureAwait(false);
            }
        }

        public async Task<(bool success, int? result)> GetBindingIdAsync(long qq)
        {
            var (success, bi) = await GetBindingInfoLegacyAsync(qq).ConfigureAwait(false);
            return (success, bi?.OsuId);
        }

        public async Task<int?> GetOsuIdAsync(long qq)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            return (await db.Bindings.AsNoTracking().FirstOrDefaultAsync(b => b.UserId == qq).ConfigureAwait(false))?.OsuId;
        }

        public Task<UserBest[]> GetUserBestRetryAsync(int userId, Mode mode, CancellationToken cancellationToken = default)
        {
            return GetUserBestLimitRetryAsync(userId, mode, 100, cancellationToken);
        }

        public Task<UserBest[]> GetUserBestLimitRetryAsync(int userId, Mode mode, int limit, CancellationToken cancellationToken = default)
        {
            var policy = Policy.Handle<Exception>(e => e is not WebApiClient.HttpStatusFailureException f || f.StatusCode == HttpStatusCode.TooManyRequests)
                .WaitAndRetryForeverAsync(i => GetExponentialBackoffDuration(i));
            return policy.ExecuteAsync(_ => _osuApiClient.GetUserBest(userId, mode, limit), cancellationToken);
        }

        public Task<UserInfo> GetUserInfoRetryAsync(int userId, Mode mode, CancellationToken cancellationToken = default)
        {
            var policy = Policy.Handle<Exception>(e => e is not WebApiClient.HttpStatusFailureException f || f.StatusCode == HttpStatusCode.TooManyRequests)
                .WaitAndRetryForeverAsync(i => GetExponentialBackoffDuration(i));
            return policy.ExecuteAsync(_ => _osuApiClient.GetUser(userId, mode), cancellationToken);
        }

        public async ValueTask<BeatmapInfo?> GetBeatmapInfoAsync(int beatmapId, Mode mode, CancellationToken cancellationToken = default)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            var cached = await db.BeatmapInfoCache.AsTracking().FirstOrDefaultAsync(c => c.BeatmapId == beatmapId && c.Mode == mode, cancellationToken).ConfigureAwait(false);
            if (cached != null)
            {
                if (cached.BeatmapInfo?.Approved is Approved.Ranked or Approved.Approved
                    || (cached.BeatmapInfo?.Approved == Approved.Loved && cached.CacheDate > DateTimeOffset.UtcNow.AddDays(-183))
                    || cached.CacheDate > DateTimeOffset.UtcNow.AddDays(-14))
                {
                    return cached.BeatmapInfo;
                }
            }
            var currentDate = DateTimeOffset.UtcNow;
            var beatmap = await _osuApiClient.GetBeatmap(beatmapId, mode).ConfigureAwait(false);

            // add to cache
            // null result is also cached
            // if the beatmap is not ranked, info may change.
            TimeSpan? expiresIn = beatmap?.Approved is Approved.Ranked or Approved.Approved
                ? null
                : beatmap?.Approved == Approved.Loved
                ? TimeSpan.FromDays(183)
                : TimeSpan.FromDays(14);
            var expireDate = DateTimeOffset.UtcNow + expiresIn;
            if (cached == null)
            {
                var newEntry = new BeatmapInfoCacheEntry
                {
                    BeatmapId = beatmapId,
                    Mode = mode,
                    CacheDate = currentDate,
                    ExpirationDate = expireDate,
                    BeatmapInfo = beatmap,
                };
                _ = db.BeatmapInfoCache.Add(newEntry);
            }
            else
            {
                cached.CacheDate = currentDate;
                cached.ExpirationDate = expireDate;
                cached.BeatmapInfo = beatmap;
            }
            try
            {
                _ = await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            }
            catch
            {
                // ignore this exception
            }

            return beatmap;
        }

        #region Utility functions
        private static TimeSpan GetExponentialBackoffDuration(int retryCount, int baseMilliseconds = 50, int maxMilliseconds = 1000)
        {
            Debug.Assert(retryCount > 0);
            Debug.Assert(baseMilliseconds > 0);
            Debug.Assert(maxMilliseconds > 0);
            var random = Random.Shared;
            return BitOperations.Log2((uint)(maxMilliseconds / baseMilliseconds)) < retryCount - 1
                ? TimeSpan.FromMilliseconds(random.Next(maxMilliseconds) + 1)
                : TimeSpan.FromMilliseconds(random.Next(baseMilliseconds << (retryCount - 1)) + 1);
        }
        #endregion
    }
}
