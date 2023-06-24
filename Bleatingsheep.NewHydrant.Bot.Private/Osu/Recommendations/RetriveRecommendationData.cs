using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Data;
using Bleatingsheep.Osu;
using Bleatingsheep.Osu.ApiClient;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sisters.WudiLib;
using static Bleatingsheep.Osu.Mods;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.Osu.Recommendations
{
#nullable enable
    [Component("RetriveRecommendationData")]
    public class RetriveRecommendationData : IMessageCommand
    {
        private static readonly IList<Mods> s_modFilters = new Mods[4]
        {
            DoubleTime | HalfTime | Easy | HardRock | Hidden | Flashlight | TouchDevice,
            DoubleTime,
            DoubleTime,
            DoubleTime,
        };

        private static readonly SemaphoreSlim s_startSemaphore = new(1);
        private static int queueCount = 0;
        private static int errorCount = 0;
        private readonly IDataProvider _dataProvider;
        private readonly NewbieContext _newbieContext;
        private readonly IDbContextFactory<NewbieContext> _dbContextFactory;
        private readonly ILogger<RetriveRecommendationData> _logger;
        private bool _start = false;
        private HttpApiClient _api = default!;
        private MessageContext _context = default!;

        public RetriveRecommendationData(IDataProvider dataProvider, NewbieContext newbieContext, IDbContextFactory<NewbieContext> dbContextFactory, ILogger<RetriveRecommendationData> logger)
        {
            _dataProvider = dataProvider;
            _newbieContext = newbieContext;
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public async Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            _api = api;
            _context = context;
            var currentCount = await _newbieContext.Recommendations.CountAsync().ConfigureAwait(false);
            await api.SendMessageAsync(context.Endpoint, $"已有 {currentCount} 条数据。").ConfigureAwait(false);

            if (!_start || !s_startSemaphore.Wait(0))
            {
                await api.SendMessageAsync(context.Endpoint, $"当前队列：{queueCount}, 错误：{errorCount}").ConfigureAwait(false);
                return;
            }

            try
            {
                await Retrive().ConfigureAwait(false);
            }
            finally
            {
                s_startSemaphore.Release();
            }
        }

        private async Task Retrive()
        {
            var mode = Mode.Standard;
            var userList = await _newbieContext.UserSnapshots.Where(s => s.Mode == mode).Select(s => new { s.UserId, s.Mode }).Distinct().ToListAsync().ConfigureAwait(false);

            // 配置
            var mustUpdatedIn = TimeSpan.FromDays(186); // 必须在此期间内更新过 BP
            var leftRange = TimeSpan.FromDays(744); // 在此期间内的作为参考BP
            var rightRange = TimeSpan.FromDays(372); // 在此期间内的BP作为推荐BP

            queueCount = userList.Count;
            errorCount = 0;
            var taskList = userList.Select(async u =>
            {
                try
                {
                    IEnumerable<UserBest> bests = await _dataProvider.GetUserBestRetryAsync(u.UserId, u.Mode).ConfigureAwait(false);
                    if (bests?.All(b => b.Date < DateTimeOffset.UtcNow.Subtract(mustUpdatedIn)) != false)
                    {// 必须在此期间内更新过 BP
                        return Array.Empty<RecommendationEntry>();
                    }
                    // only get bps in recent half year.
                    var filteredBest = bests.Select((b, i) => (b, i)).ToList();
                    return (IList<RecommendationEntry>)
                    (from x1 in filteredBest.Where(x => x.b.Date >= DateTimeOffset.UtcNow.Subtract(leftRange))
                     from x2 in filteredBest.Where(x => x.b.Date >= DateTimeOffset.UtcNow.Subtract(rightRange))
                     where x1.b.Date > x2.b.Date
                     let recommendationDegree = Math.Pow(0.95, x1.i + x2.i - 1)
                     select new RecommendationEntry
                     {
                         Mode = u.Mode,
                         Left = RecommendationBeatmapId.Create(x1.b, u.Mode),
                         Recommendation = RecommendationBeatmapId.Create(x2.b, u.Mode),
                         RecommendationDegree = recommendationDegree,
                         Performance = x2.b.Performance,
                     }).ToList();
                }
                catch (Exception e)
                {
                    var v = Interlocked.Increment(ref errorCount);
                    if (v <= 1)
                    {
                        await _api.SendMessageAsync(_context.Endpoint, e.ToString()).ConfigureAwait(false);
                    }
                    return Array.Empty<RecommendationEntry>();
                }
                finally
                {
                    Interlocked.Decrement(ref queueCount);
                }
            });
            var r = await Task.WhenAll(taskList).ConfigureAwait(false);
            var expanded = r.SelectMany(e => e)
                .OrderBy(e => ((ulong)(uint)e.Left.GetHashCode() << 32) | ((uint)e.Recommendation.GetHashCode() & 0xffffffffUL))
                .ToList();
            var expandedZeroCount = expanded.Count(e => e.RecommendationDegree == 0);
            _logger.LogInformation("展开完成，共 {expandedCount} 项，{expandedZeroCount} 项的推荐度为 0。", expanded, expandedZeroCount);
            var recommendations = MergeRecommendationEnumerable(expanded, mode);

            // 先清除当前的推荐数据，再添加新的
            var deleteCount = await _newbieContext.Recommendations.Where(r => r.Mode == mode).ExecuteDeleteAsync();
            _logger.LogInformation("清除 {deleteCount} 条旧的推荐图数据。", deleteCount);

            var recommendationsChunks = recommendations.Chunk(5000);
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
            await Parallel.ForEachAsync(recommendationsChunks, parallelOptions, async (chunk, cancellationToken) =>
            {
                // 不这么干不行，因为 EF Core 追踪实体需要消耗大量内存。
                // 现在这样可以限制同时追踪的实体数，10000 条消耗大约 600M 内存。
                // 实际限制的数量应根据内存大小和 CPU 核心数调整。
                var toAdd = chunk;
                await using var db = _dbContextFactory.CreateDbContext();
                db.Recommendations.AddRange(toAdd);
                await db.SaveChangesAsync(cancellationToken);
            });
        }

        public IEnumerable<RecommendationEntry> MergeRecommendationEnumerable(IList<RecommendationEntry> expanded, Mode mode)
        {
            var recent = expanded.FirstOrDefault();
            var degree = 0.0;
            var performance = 0.0;
            foreach (var current in expanded)
            {
                if ((current.Left, current.Recommendation) == (recent!.Left, recent.Recommendation))
                {
                    degree += current.RecommendationDegree;
                    performance += current.Performance * current.RecommendationDegree;
                }
                else
                {
                    yield return new RecommendationEntry
                    {
                        Mode = mode,
                        Left = recent.Left,
                        Recommendation = recent.Recommendation,
                        RecommendationDegree = degree,
                        Performance = performance / degree,
                    };
                    degree = current.RecommendationDegree;
                    performance = current.Performance * current.RecommendationDegree;
                    recent = current;
                }
            }
        }

        public bool ShouldResponse(MessageContext context)
        {
            if (context.UserId == 962549599)
            {
                if (context.Content.Text == "采集数据")
                    return true;
                if (context.Content.Text == "开始采集数据")
                {
                    _start = true;
                    return true;
                }
            }
            return false;
        }
    }
#nullable restore
}
