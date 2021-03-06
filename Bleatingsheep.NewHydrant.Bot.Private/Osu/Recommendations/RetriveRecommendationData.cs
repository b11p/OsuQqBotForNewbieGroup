﻿using System;
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
using Sisters.WudiLib;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;
using static Bleatingsheep.Osu.Mods;

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

        private bool _start = false;
        private HttpApiClient _api = default!;
        private MessageContext _context = default!;

        public RetriveRecommendationData(IDataProvider dataProvider, NewbieContext newbieContext)
        {
            _dataProvider = dataProvider;
            _newbieContext = newbieContext;
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
            queueCount = userList.Count;
            errorCount = 0;
            var taskList = userList.Select(async u =>
            {
                try
                {
                    IEnumerable<UserBest> bests = await _dataProvider.GetUserBestRetryAsync(u.UserId, u.Mode).ConfigureAwait(false);
                    if (bests?.All(b => b.Date < DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(31))) != false)
                    {// must have bp updated in recent 31 days.
                        return Array.Empty<RecommendationEntry>();
                    }
                    // only get bps in recent half year.
                    var filteredBest = bests.Select((b, i) => (b, i)).Where(x => x.b.Date >= DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(186))).ToList();
                    return (IList<RecommendationEntry>)
                    (from x1 in filteredBest
                     from x2 in filteredBest
                     where x1.b.Date > x2.b.Date
                     select new RecommendationEntry
                     {
                         Mode = u.Mode,
                         Left = RecommendationBeatmapId.Create(x1.b, u.Mode),
                         Recommendation = RecommendationBeatmapId.Create(x2.b, u.Mode),
                         RecommendationDegree = Math.Pow(0.95, x1.i + x2.i - 2),
                     }).ToList();
                }
                catch (Exception e)
                {
                    var v = Interlocked.Increment(ref errorCount);
                    if (v <= 1)
                    {
                        await _api.SendMessageAsync(_context.Endpoint, e.ToString()).ConfigureAwait(false);
                    }
                }
                finally
                {
                    Interlocked.Decrement(ref queueCount);
                }
                return Array.Empty<RecommendationEntry>();
            });
            var r = await Task.WhenAll(taskList).ConfigureAwait(false);
            var expanded = r.SelectMany(e => e)
                .OrderBy(e => ((long)e.Left.GetHashCode() << 32) | (e.Recommendation.GetHashCode() & 0xffffffff))
                .ToList();
            await _api.SendMessageAsync(_context.Endpoint, $"展开完成，共{expanded.Count}项。").ConfigureAwait(false);
            var recent = expanded.FirstOrDefault();
            var degree = 0.0;
            var recommendationList = new List<RecommendationEntry>();
            foreach (var current in expanded)
            {
                if ((current.Left, current.Recommendation) == (recent!.Left, recent.Recommendation))
                {
                    degree += current.RecommendationDegree;
                }
                else
                {
                    recommendationList.Add(new RecommendationEntry
                    {
                        Mode = mode,
                        Left = recent.Left,
                        Recommendation = recent.Recommendation,
                        RecommendationDegree = degree,
                    });
                    degree = 0;
                    recent = current;
                }
            }
            var recommendations = recommendationList.ToArray();
            //await _newbieContext.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            //{
            //    await using var transaction = await _newbieContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable).ConfigureAwait(false);
            //var oldData = await _newbieContext.Recommendations.ToListAsync().ConfigureAwait(false);
            //_newbieContext.Recommendations.RemoveRange(oldData);
            //await _newbieContext.SaveChangesAsync().ConfigureAwait(false);
            for (int i = 0; i * 10000 < recommendations.Length; i++)
            {
                // 不这么干不行，因为 EF Core 追踪实体需要消耗大量内存。
                // 现在这样可以把同时追踪的实体数限制在一万，消耗大约 600M 内存。
                var toAdd = (i + 1) * 10000 > recommendations.Length
                    ? recommendations[(i * 10000)..]
                    : recommendations[(i * 10000)..((i + 1) * 10000)];
                _newbieContext.Recommendations.AddRange(toAdd);
                await _newbieContext.SaveChangesAsync().ConfigureAwait(false);

                var changedEntriesCopy = _newbieContext.ChangeTracker.Entries()
                    .Where(e => e.State == EntityState.Added ||
                                e.State == EntityState.Modified ||
                                e.State == EntityState.Deleted)
                    .ToList();
                foreach (var entry in changedEntriesCopy)
                    entry.State = EntityState.Detached;
            }
            //_newbieContext.Recommendations.AddRange(recommendationList);
            //await _newbieContext.SaveChangesAsync().ConfigureAwait(false);
            //    await transaction.CommitAsync().ConfigureAwait(false);
            //}).ConfigureAwait(false);
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
