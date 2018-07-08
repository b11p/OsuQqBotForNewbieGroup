using Bleatingsheep.OsuMixedApi;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bleatingsheep.OsuQqBot.Database
{
    public static class NewbieDatabase
    {
        /// <exception cref="NewbieDbException"></exception>
        private static T TryUsingContext<T>(Func<NewbieContext, T> func)
        {
            try
            {
                using (var context = new NewbieContext())
                {
                    return func(context);
                }
            }
            catch (Exception e)
            {
                throw new NewbieDbException(e.Message, e);
            }
        }

        /// <exception cref="NewbieDbException"></exception>
        private static async Task<T> TryUsingContextAsync<T>(Func<NewbieContext, Task<T>> func)
        {
            try
            {
                using (var context = new NewbieContext())
                {
                    return await func(context);
                }
            }
            catch (Exception e)
            {
                throw new NewbieDbException(e.Message, e);
            }
        }

        public static Chart AddChart(Chart chart)
        {
            using (var context = new NewbieContext())
            {
                Chart result;
                try
                {
                    result = context.Charts.Add(chart).Entity;
                    context.SaveChanges();
                    return result;
                }
                catch (InvalidOperationException)
                {
                    return null;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public static (Chart chart, ChartTry[] commits)[] Commits(long groupId, int uid)
        {
            using (var context = new NewbieContext())
            {
                var now = DateTimeOffset.Now;
                var chart_commit = (from chart in context.Charts
                                    from commit in context.ChartTries
                                    where (from g in context.ChartValidGroups
                                           where g.GroupId == groupId
                                           select g.ChartId).Contains(chart.ChartId)
                                          && commit.UserId == uid
                                          && chart.ChartId == commit.ChartId
                                    group commit by chart).ToArray();
                return chart_commit
                    .Select(g => (g.Key, g.ToArray()))
                    .OrderBy(t => t.Item1.ChartId)
                    .ToArray();
            }
        }

        private static async Task<Chart> GetChartAsync(int chartId, Func<IQueryable<Chart>, IQueryable<Chart>> includes = null)
        {
            using (var context = new NewbieContext())
            {
                IQueryable<Chart> charts = context.Charts;
                if (includes != null) charts = includes(charts);
                return await charts.SingleOrDefaultAsync(c => c.ChartId == chartId);
            }
        }

        public static async Task<Chart> GetChartWithCommitsAsync(int chartId)
        {
            return await GetChartAsync(chartId, charts => charts.Include(c => c.Maps).ThenInclude(map => map.Commits));
        }

        public static CommitResult
        Commit(
            long groupId, PlayRecord record, double performance, out Chart[] commitedCharts
        )
        {
            commitedCharts = null;
            Chart[] chartsInThisGroup;
            Dictionary<int, ChartBeatmap> beatmaps;
            using (var context = new NewbieContext())
            {
                // 获取当前群中标记了进行的 Chart。
                chartsInThisGroup = (from c in context.Charts
                                     where (from g in context.ChartValidGroups
                                            where g.GroupId == groupId
                                            select g.ChartId).Contains(c.ChartId)
                                            && c.IsRunning
                                     select c).ToArray();
                // 如果没有合适的 Chart，直接返回。
                if (!chartsInThisGroup.Any()) return CommitResult.NoChart;
                beatmaps = chartsInThisGroup.ToDictionary(
                    c => c.ChartId,
                    c => context.ChartBeatmaps.FirstOrDefault(
                        m => m.ChartId == c.ChartId && m.BeatmapId == record.BeatmapId && m.Mode == record.Mode
                    )
                );
            }
            var chartResults = chartsInThisGroup.GroupBy(c =>
            {
                // 判断已结束
                if (c.EndTime <= record.DateOffset) return CommitResult.DateOverLimit;
                // 判断 PP
                if (c.MaximumPerformance < performance) return CommitResult.PerformanceOverLimit;
                // 从数据库查找地图（及模式）
                var beatmap = beatmaps.GetValueOrDefault(c.ChartId);
                // 判断是否有这个图
                if (beatmap == null)
                    return CommitResult.NoBeatmapForYou;
                // 判断已开始
                if (record.DateOffset < c.StartTime) return CommitResult.ChartNotStarted;
                // 判断 Mod
                if (!record.Mods.HasFlag(beatmap.RequiredMods)) return CommitResult.ModNotAllowed;
                if (beatmap.ForceMods != Mods.None && (beatmap.ForceMods & record.Mods) == Mods.None)
                    return CommitResult.ModNotAllowed;
                if ((beatmap.BannedMods & record.Mods) != Mods.None)
                    return CommitResult.ModNotAllowed;
                // 判断 Fail
                if (!beatmap.AllowsFail && !record.Pass) return CommitResult.Failed;
                // OK
                return CommitResult.Success;
            }).OrderBy(g => (int)g.Key);
            foreach (var group in chartResults)
            {
                if (group.Key != CommitResult.Success) return group.Key;
                //int commited = 0;
                var commited = new List<Chart>();
                foreach (var chart in group)
                {
                    using (var context = new NewbieContext())
                    {
                        try
                        {
                            context.ChartTries.Add(ChartTry.FromRecord(beatmaps[chart.ChartId], record, performance));
                            context.SaveChanges();
                            //commited++;
                            commited.Add(chart);
                        }
                        catch (DbUpdateException e)
                        //when (e.InnerException is Microsoft.Data.Sqlite.SqliteException)
                        { }
                    }
                }
                //if (commited == 0) return CommitResult.CommitedException;
                if (commited.Count == 0) return CommitResult.CommitedException;
                commitedCharts = commited.ToArray();
                return CommitResult.Success;
            }
            return CommitResult.UnknownException;
        }

        public static (Chart chart, ChartBeatmap[] maps)[] ChartInGroup(long groupId)
        {
            using (var context = new NewbieContext())
            {
                // TODO: 
                // 目前，不会显示没有地图的 Chart，
                // 但是，我希望显示。
                var c_b = (from c in context.Charts
                           from b in context.ChartBeatmaps
                           where (from g in context.ChartValidGroups
                                  where g.GroupId == groupId
                                  select g.ChartId).Contains(c.ChartId)
                                  && c.IsRunning
                                  && c.ChartId == b.ChartId
                           group b by c).ToArray();
                return c_b.Select(g => (g.Key, g.ToArray())).ToArray();
            }
        }

        /// <summary>
        /// 绑定 QQ 号和 osu! 账号。
        /// </summary>
        /// <param name="qq">要绑定的 QQ 号。</param>
        /// <param name="osuId">要绑定的 osu! UID。</param>
        /// <param name="osuName">记录在日志里的 osu! 用户名。</param>
        /// <param name="source">绑定来源。记录在日志和绑定信息里。</param>
        /// <param name="operatorId">操作者 QQ 号。记录在日志里。</param>
        /// <param name="operatorName">操作者名字。记录在日志里。</param>
        /// <exception cref="NewbieDbException">绑定过程出现异常。</exception>
        /// <returns>以前的 osu! UID。</returns>
        public static async Task<int?> BindAsync(long qq, int osuId, string osuName, string source, long? operatorId, string operatorName)
        {
            try
            {
                using (var context = new NewbieContext())
                {
                    var bindingInfo = await context.Bindings.SingleOrDefaultAsync(b => b.UserId == qq);
                    var oldOsuId = bindingInfo?.OsuId;
                    if (bindingInfo is BindingInfo)
                    {
                        if (bindingInfo.OsuId == osuId)
                        {
                            return osuId;
                        }
                        bindingInfo.OsuId = osuId;
                        bindingInfo.Source = source;
                    }
                    else
                    {
                        bindingInfo = new BindingInfo { OsuId = osuId, UserId = qq, Source = source };
                        await context.Bindings.AddAsync(bindingInfo);
                    }
                    await context.Histories.AddAsync(new OperationHistory
                    {
                        Operation = Operation.Binding,
                        UserId = qq,
                        User = osuName,
                        OperatorId = operatorId,
                        Operator = operatorName,
                        Remark = $"osu! ID: {osuId}; source: {source}",
                    });
                    await context.SaveChangesAsync();
                    return oldOsuId;
                }
            }
            catch (Exception e)
            {
                throw new NewbieDbException(e.Message, e);
            }
        }

        /// <summary>
        /// 获取绑定信息。
        /// </summary>
        /// <param name="qq">QQ 号。</param>
        /// <exception cref="NewbieDbException"></exception>
        /// <returns>绑定信息。如果没绑定，则为 <c>null</c>。</returns>
        public static async Task<BindingInfo> GetBindingInfoAsync(long qq)
        {
            return await TryUsingContextAsync(async context =>
            {
                return await context.Bindings.SingleOrDefaultAsync(b => b.UserId == qq);
            });
        }

        /// <exception cref="NewbieDbException"></exception>
        public static async Task<int?> GetBindingIdAsync(long qq)
        {
            return (await GetBindingInfoAsync(qq))?.OsuId;
        }

        public static async Task<Beatmap> GetBeatmapAsync(int bid, Mode mode)
        {
            using (var context = new NewbieContext())
            {
                return await context.CachedBeatmaps.SingleOrDefaultAsync(b => b.Id == bid && b.Mode == mode);
            }
        }

        public static async Task<Beatmap> CacheBeatmapAsync(Beatmap beatmap)
        {
            using (var context = new NewbieContext())
            {

                try
                {
                    var result = await context.CachedBeatmaps.AddAsync(beatmap);
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    return null;
                }
                return beatmap;
            }
        }
    }

    /// <summary>
    /// 提交 Chart 的结果。
    /// 越靠前的越被认为可以优先满足。
    /// </summary>
    public enum CommitResult
    {
        /// <summary>
        /// 提交成功。
        /// </summary>
        Success,
        /// <summary>
        /// 你已经提交过了。
        /// </summary>
        CommitedException,
        /// <summary>
        /// 你 Fail 了，所以不允许提交。
        /// </summary>
        Failed,
        /// <summary>
        /// 选择的 Mod 不符合要求。
        /// </summary>
        ModNotAllowed,
        /// <summary>
        /// 打图时间早于 Chart开始时间。
        /// </summary>
        ChartNotStarted,
        /// <summary>
        /// 你打的图不在任何一个 Chart 图池中。
        /// </summary>
        NoBeatmapForYou,
        /// <summary>
        /// PP 超过最高限制。
        /// </summary>
        PerformanceOverLimit,
        /// <summary>
        /// 符合条件的 Chart 已结束。
        /// </summary>
        DateOverLimit,
        /// <summary>
        /// 本群没有合适的 Chart。
        /// </summary>
        NoChart,
        /// <summary>
        /// 未知异常。
        /// </summary>
        UnknownException,
    }
}
