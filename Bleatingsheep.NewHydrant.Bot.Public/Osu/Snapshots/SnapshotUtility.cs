using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Bleatingsheep.Osu;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Bleatingsheep.NewHydrant.Osu.Snapshots
{
    public class SnapshotUtility : OsuFunction
    {
        /// <summary>
        /// Query user play records.
        /// Parameters must have same number of elements,
        /// and there must not be any duplicate <c>userId</c> and <c>mode</c> combination.
        /// </summary>
        /// <param name="context">Database context to use.</param>
        /// <param name="userIds">List of queried user ids. <c>userIds</c>, <c>modes</c> and <c>startIndecies</c> must have same number of elements.</param>
        /// <param name="modes">List of queried modes. <c>userIds</c>, <c>modes</c> and <c>startIndecies</c> must have same number of elements.</param>
        /// <param name="startIndecies">List of first numbers played. <c>userIds</c>, <c>modes</c> and <c>startIndecies</c> must have same number of elements.</param>
        /// <returns>A list of user play records.</returns>
        public static async Task<IList<UserPlayRecord>> GetUserPlayRecordsAsync(
            NewbieContext context,
            IEnumerable<int> userIds,
            IEnumerable<Mode> modes,
            IEnumerable<int> startIndecies)
        {
            var tracking = context.ChangeTracker.QueryTrackingBehavior;
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            // TODO: Verify that there is no repeating conditions (with same userId and mode).
            // If so, select the minimum of StartNumber.
            var filters = userIds.Zip(modes).Zip(startIndecies, (tuple, start) =>
            {
                var (userId, mode) = tuple;
                return new PlayRecordQueryTemp
                {
                    UserId = userId,
                    Mode = mode,
                    StartNumber = start,
                };
            });

            var result = await context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                using (await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted).ConfigureAwait(false))
                {
                    await context.PlayRecordQueryTemps.AddRangeAsync(filters).ConfigureAwait(false);
                    await context.SaveChangesAsync().ConfigureAwait(false);
                    return await (from r in context.UserPlayRecords
                                  join f in context.PlayRecordQueryTemps on new { r.UserId, r.Mode } equals new { f.UserId, f.Mode }
                                  where r.PlayNumber >= f.StartNumber
                                  select r).ToListAsync().ConfigureAwait(false);
                }
            }).ConfigureAwait(false);

            context.ChangeTracker.QueryTrackingBehavior = tracking;
            return result;
        }
    }
}
