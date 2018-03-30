using Bleatingsheep.OsuMixedApi;
using Bleatingsheep.OsuQqBot.Database.Models;
using System.Linq;

namespace Bleatingsheep.OsuQqBot.Database
{
    public static class NewbieDatabase
    {
        public static CommitResult Commit(long groupId, PlayRecord record, double performance)
        {
            using (var context = new NewbieContext())
            {
                var chartsInThisGroup = from c in context.Charts
                                        where (from g in context.ChartValidGroups
                                               where g.GroupId == groupId
                                               select g.ChartId).Contains(c.ChartId)
                                               && c.IsRunning
                                        select c;
                var chartResults= chartsInThisGroup.GroupBy(c =>
                {
                    if (c.EndTime <= record.DateOffset) return CommitResult.DateOverLimit;
                });
                var qCharts = from c in chartsInThisGroup
                              where c.StartTime <= record.DateOffset
                              && (c.EndTime == null || c.EndTime.Value > record.DateOffset)
                              && c.MaximumPerformance == null || c.MaximumPerformance.Value > performance
                              select c;
            }
        }
    }

    /// <summary>
    /// 提交 Chart 的结果。
    /// </summary>
    public enum CommitResult
    {
        /// <summary>
        /// 提交成功。
        /// </summary>
        Success,
        /// <summary>
        /// 没有合适的 Chart。
        /// </summary>
        NoChart,
        /// <summary>
        /// 选择的 Mod 不符合要求。
        /// </summary>
        ModNotAllowed,
        /// <summary>
        /// 打图时间早于 Chart开始时间。
        /// </summary>
        ChartNotStarted,
        /// <summary>
        /// PP 超过最高限制。
        /// </summary>
        PerformanceOverLimit,
        /// <summary>
        /// 符合条件的 Chart 已结束。
        /// </summary>
        DateOverLimit,
    }
}
