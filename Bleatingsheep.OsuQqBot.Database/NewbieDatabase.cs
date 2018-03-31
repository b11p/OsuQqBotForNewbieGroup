using Bleatingsheep.OsuMixedApi;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bleatingsheep.OsuQqBot.Database
{
    public static class NewbieDatabase
    {
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

        public static CommitResult Commit(long groupId, PlayRecord record, double performance)
        {
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
                        m => m.ChartId == c.ChartId && m.BeatmapId == record.Bid && m.Mode == record.Mode
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
                int commited = 0;
                foreach (var chart in group)
                {
                    using (var context = new NewbieContext())
                    {
                        try
                        {
                            context.ChartCommits.Add(ChartCommit.FromRecord(beatmaps[chart.ChartId], record, performance));
                            context.SaveChanges();
                            commited++;
                        }
                        catch (DbUpdateException e)
                        when (e.InnerException is Microsoft.Data.Sqlite.SqliteException)
                        { }
                    }
                }
                if (commited == 0) return CommitResult.CommitedException;
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
                                  //&& c.IsRunning
                                  && c.ChartId == b.ChartId
                           group b by c).ToArray();
                return c_b.Select(g => (g.Key, g.ToArray())).ToArray();
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
