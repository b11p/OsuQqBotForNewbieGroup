using System;
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
        public async Task UpdateAsync(int osuId, Mode mode)
        {
            // 开始通过 API 获取用户信息和最近游玩记录。
            using var osuApi = CreateOsuApi();
            var getUserTask = osuApi.GetUser(osuId, mode);
            var getRecentTask = osuApi.GetUserRecent(osuId, mode, 100);

            // Create database context instance.
            using var dbContext = new NewbieContext();
            dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var snap = await (from s in dbContext.UserSnapshots
                              where s.UserId == osuId && s.Mode == mode
                              orderby s.Date descending
                              select s.UserInfo).FirstOrDefaultAsync().ConfigureAwait(false);

            // Save snapshot to database.
            var userInfo1 = await getUserTask.ConfigureAwait(false);
            if (userInfo1 is null) return;
            if (userInfo1.PlayCount == snap?.PlayCount && userInfo1.Performance == 0) return; // Inactive player.
            userInfo1.Events = null; // Set to null to help compare.
            if (!PropertyEquals(userInfo1, snap))
            {
                await dbContext.UserSnapshots.AddAsync(UserSnapshot.Create(osuId, mode, userInfo1)).ConfigureAwait(false);
                await dbContext.SaveChangesAsync().ConfigureAwait(false);
            }

            var userRecent = await getRecentTask.ConfigureAwait(false);
            if (userRecent is null || userRecent.Length == 0)
            {// The user didn't play, no need to check play number.
                return;
            }

            // Confirm play number.
            var userInfo2 = await osuApi.GetUser(osuId, mode).ConfigureAwait(false);
            if (userInfo2?.PlayCount == userInfo1.PlayCount)
            {
                var records = userRecent.OrderByDescending(r => r.Date)
                    .Zip(CreateDescendingSequence(userInfo1.PlayCount),
                         (r, n) => UserPlayRecord.Create(osuId, mode, n, r))
                    .ToList();

                var currentMinPlayNumber = records.Last().PlayNumber;

                var strategy = dbContext.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    // Filter existing data.
                    var existed = await dbContext.UserPlayRecords
                        .Where(r => r.UserId == osuId && r.Mode == mode && r.PlayNumber >= currentMinPlayNumber)
                        .Select(r => r.PlayNumber)
                        .ToListAsync().ConfigureAwait(false);
                    var inserting = records.Where(r => !existed.Contains(r.PlayNumber)).ToList();

                    // Insert data.
                    if (inserting.Count > 0)
                    {
                        await dbContext.UserPlayRecords.AddRangeAsync(inserting).ConfigureAwait(false);
                        await dbContext.SaveChangesAsync().ConfigureAwait(false);
                    }
                }).ConfigureAwait(false);
            }
            else
            {
                Logger.Info($"Concurrent error! Get different play_count in two queries. ({userInfo1.PlayCount}, {userInfo2?.PlayCount})");
                Logger.Info($"The user is {userInfo1.Name}({userInfo1.Id}); the mode is {mode}");
            }
        }

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

        /// <summary>
        /// Create a sequence that starts at the spicified number and with step -1.
        /// All elements are positive or zero.
        /// </summary>
        /// <param name="start"></param>
        /// <returns>A descending sequence. All elements are non-negative.</returns>
        private static IEnumerable<int> CreateDescendingSequence(int start)
        {
            for (; start >= 0; start--)
            {
                yield return start;
            }
        }

        private static bool PropertyEquals<T>(T left, T right) => PropertyEquals(left, right, typeof(T));

        private static bool PropertyEquals(object left, object right, Type type)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null && right is null) return true;
            if (left is null || right is null) return false;
            if (!(type.IsInstanceOfType(left) && type.IsInstanceOfType(right)))
                throw new InvalidOperationException("Type error!");
            var properties = type.GetProperties();
            return properties.Where(p => p.CanRead).All(p => Equals(p.GetValue(left), p.GetValue(right)));
        }
    }
}
