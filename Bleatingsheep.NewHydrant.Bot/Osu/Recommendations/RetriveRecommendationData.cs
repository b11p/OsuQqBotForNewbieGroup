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

        public RetriveRecommendationData(IDataProvider dataProvider, NewbieContext newbieContext)
        {
            _dataProvider = dataProvider;
            _newbieContext = newbieContext;
        }

        public async Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
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
            var userList = await _newbieContext.UserSnapshots.Select(s => new { s.UserId, s.Mode }).Distinct().ToListAsync().ConfigureAwait(false);
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
                    return (IEnumerable<RecommendationEntry>)
                    (from x1 in filteredBest
                     from x2 in filteredBest
                     where x1.b.Date > x2.b.Date
                     select new RecommendationEntry
                     {
                         Left = RecommendationBeatmapId.Create(x1.b, u.Mode),
                         Recommendation = RecommendationBeatmapId.Create(x2.b, u.Mode),
                         RecommendationDegree = Math.Pow(0.95, x1.i + x2.i - 2),
                     });
                }
                catch (Exception)
                {
                    Interlocked.Increment(ref errorCount);
                }
                finally
                {
                    Interlocked.Decrement(ref queueCount);
                }
                return Array.Empty<RecommendationEntry>();
            });
            var r = await Task.WhenAll(taskList).ConfigureAwait(false);
            var result = from e in r
                         from entry in e
                         group entry.RecommendationDegree by (entry.Left, entry.Recommendation)
                             into g
                         select new RecommendationEntry
                         {
                             Left = g.Key.Left,
                             Recommendation = g.Key.Recommendation,
                             RecommendationDegree = g.Sum(),
                         };
            await using var transaction = await _newbieContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable).ConfigureAwait(false);
            await _newbieContext.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                var oldData = await _newbieContext.Recommendations.ToListAsync().ConfigureAwait(false);
                _newbieContext.Recommendations.RemoveRange(oldData);
                await _newbieContext.Recommendations.AddRangeAsync(result).ConfigureAwait(false);
                await _newbieContext.SaveChangesAsync().ConfigureAwait(false);
                await transaction.CommitAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);
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