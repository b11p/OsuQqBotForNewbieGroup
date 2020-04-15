using System;
using System.Collections.Generic;
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
            await dbContext.UserSnapshots.AddAsync(UserSnapshot.Create(osuId, mode, userInfo1)).ConfigureAwait(false);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);

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
                var retries = 4 - 1;
                while (true)
                {
                    using var t = await dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable).ConfigureAwait(false);
                    try
                    {
                        // Filter existing data.
                        var existed = await dbContext.UserPlayRecords
                            .Where(r => r.UserId == osuId && r.Mode == mode && r.PlayNumber >= currentMinPlayNumber)
                            .Select(r => r.PlayNumber)
                            .ToListAsync().ConfigureAwait(false);
                        var inserting = records.Where(r => !existed.Contains(r.PlayNumber));

                        // Insert data.
                        await dbContext.UserPlayRecords.AddRangeAsync(inserting).ConfigureAwait(false);
                        await dbContext.SaveChangesAsync().ConfigureAwait(false);
                        await t.CommitAsync().ConfigureAwait(false);
                        break;
                    }
                    catch (DbUpdateConcurrencyException) when (retries-- > 0)
                    {
                        await t.RollbackAsync().ConfigureAwait(false);
                    }
                }
                Console.WriteLine(); // Check endless loop.
            }
            else
            {
                Logger.Info($"Concurrent error! Get different play_count in two queries. ({userInfo1.PlayCount}, {userInfo2?.PlayCount})");
                Logger.Info($"The user is {userInfo1.Name}({userInfo1.Id}); the mode is {mode}");
            }
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
    }
}
