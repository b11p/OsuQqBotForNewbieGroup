using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Bleatingsheep.Osu;
using Bleatingsheep.Osu.ApiClient;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bleatingsheep.NewHydrant.Data
{
    public class DataMaintainer
    {
        private readonly IDbContextFactory<NewbieContext> _contextFactory;
        private readonly IOsuApiClient _osuApiClient;
        private readonly ILogger<DataMaintainer> _logger;

        public DataMaintainer(IDbContextFactory<NewbieContext> contextFactory, IOsuApiClient osuApiClient, ILogger<DataMaintainer> logger)
        {
            _contextFactory = contextFactory;
            _osuApiClient = osuApiClient;
            _logger = logger;
        }

        public async Task UpdateAsync(int osuId, Mode mode)
        {
            await using var dbContext = _contextFactory.CreateDbContext();
            var schedule = await dbContext.UpdateSchedules.Where(s => s.UserId == osuId && s.Mode == mode).FirstOrDefaultAsync().ConfigureAwait(false);
            if (schedule == null)
            {
                schedule = new UpdateSchedule
                {
                    UserId = osuId,
                    Mode = mode,
                    NextUpdate = DateTimeOffset.UnixEpoch,
                };
                _ = dbContext.Add(schedule);
            }
            else
            {
                schedule.NextUpdate = DateTimeOffset.UnixEpoch;
            }
            try
            {
                _ = await dbContext.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (DbUpdateConcurrencyException)
            {
                // ignore this exception.
            }
        }

        public async Task<UpdateReport> UpdateNowAsync(int osuId, Mode mode)
        {
            // 开始通过 API 获取用户信息和最近游玩记录。
            var osuApi = _osuApiClient;
            var getUserTask = osuApi.GetUser(osuId, mode);

            var report = new UpdateReport
            {
                UserRecents = Array.Empty<UserRecent>(),
                AddedPlayRecords = Array.Empty<UserPlayRecord>(),
            };

            // Create database context instance.
            using var dbContext = _contextFactory.CreateDbContext();
            dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var snap = await (from s in dbContext.UserSnapshots
                              where s.UserId == osuId && s.Mode == mode
                              orderby s.Date descending
                              select s.UserInfo).FirstOrDefaultAsync().ConfigureAwait(false);

            // Save snapshot to database.
            var userInfo1 = await getUserTask.ConfigureAwait(false);
            if (userInfo1 is null)
            {
                report.UserNotExists = true;
                return report;
            }
            if (userInfo1.PlayCount == snap?.PlayCount && userInfo1.Performance == 0)
            {
                // Inactive player.
                report.Inactive = true;
                return report;
            }
            userInfo1.Events = null; // Set to null to help compare.
            if (!PropertyEquals(userInfo1, snap))
            {
                await dbContext.UserSnapshots.AddAsync(UserSnapshot.Create(osuId, mode, userInfo1)).ConfigureAwait(false);
                await dbContext.SaveChangesAsync().ConfigureAwait(false);
                report.AddedSnapshot = true;
            }

            var userRecent = await osuApi.GetUserRecent(osuId, mode, 100).ConfigureAwait(false);
            if (userRecent is null || userRecent.Length == 0)
            {// The user didn't play, no need to check play number.
                return report;
            }
            report.UserRecents = userRecent;
            // 确保按日期升序排列
            Array.Sort(userRecent, (a, b) => a.Date.CompareTo(b.Date));

            // Confirm play number.
            var userInfo2 = await osuApi.GetUser(osuId, mode).ConfigureAwait(false);
            if (userInfo2?.PlayCount == userInfo1.PlayCount)
            {
                var nowMinus2Day = DateTimeOffset.UtcNow.AddDays(-2);
                // 获取已有的游玩记录，按时间倒序
                var existingRecords = await dbContext.UserPlayRecords
                    .Where(r => r.UserId == osuId && r.Mode == mode)
                    .OrderByDescending(r => r.Record.Date)
                    .Take(userRecent.Length + 1)
                    .ToListAsync();

                // 获取当前最大游玩编号
                var existingMaxPlayNumber = existingRecords.Count > 0
                    ? existingRecords.Max(r => r.PlayNumber)
                    : 0;
                if (existingMaxPlayNumber > userInfo1.PlayCount)
                {
                    // 这种情况可能是号数据被重置等，比较少见
                    _logger.LogInformation("API返回的游玩次数少于记录中的最大 PlayNumber，Uid {uid}, Mode {mode}", osuId, mode);
                    existingMaxPlayNumber = existingRecords.Where(r => r.PlayNumber <= userInfo1.PlayCount).Max(r => r.PlayNumber);
                }

                ArraySegment<UserRecent> recentToAdd = userRecent.Where(r => !existingRecords.Any(e => PropertyEquals(r, e.Record))).ToArray();
                if (recentToAdd.Count == 0)
                {
                    // 全部已添加，什么也不做。
                    return report;
                }

                var toAdd = new List<UserPlayRecord>();
                // 判断新获取的是否比之前的都更新
                if (existingRecords.Count != 0 && recentToAdd[0].Date <= existingRecords[0].Record.Date)
                {
                    _logger.LogWarning("API返回的数据有早于数据库记录的信息，Uid {uid}, Mode {mode}", osuId, mode);
                    int newerStartIndex = 0;
                    for (; recentToAdd[newerStartIndex].Date <= existingRecords[0].Record.Date; newerStartIndex++)
                    { }
                    toAdd.AddRange(recentToAdd[..newerStartIndex].Select(r => UserPlayRecord.Create(osuId, mode, existingMaxPlayNumber, r)));
                    recentToAdd = recentToAdd[newerStartIndex..];
                }

                if (recentToAdd.Count >= 90)
                    _logger.LogInformation("用户{osuId}在模式{mode}的游玩数量超过阈值，请注意漏数据风险。游玩数量为{recentToAdd_Count}", osuId, mode, recentToAdd.Count);
                if (recentToAdd.Count >= 100)
                    _logger.LogWarning("用户{osuId}在模式{mode}的游玩数量超过阈值，请注意漏数据风险。游玩数量为{recentToAdd_Count}", osuId, mode, recentToAdd.Count);

                // 根据最新的玩家 PC 数和数据库中已有的最大 PC 数计算新的游玩记录数量，按号添加
                var sequentialToAdd = recentToAdd.Count <= userInfo1.PlayCount - existingMaxPlayNumber
                    ? recentToAdd
                    : recentToAdd[..(userInfo1.PlayCount - existingMaxPlayNumber)];
                toAdd.AddRange(sequentialToAdd.Reverse().Zip(CreateDescendingSequence(userInfo1.PlayCount),
                             (r, n) => UserPlayRecord.Create(osuId, mode, n, r)));
                if (sequentialToAdd.Count < recentToAdd.Count)
                {
                    _logger.LogInformation("API 返回的游玩记录数量超过游玩次数增量，Uid {uid}, Mode {mode}", osuId, mode);
                    // API 返回的数量超过游玩次数增量。
                    var extraToAdd = recentToAdd[(userInfo1.PlayCount - existingMaxPlayNumber)..];
                    toAdd.AddRange(extraToAdd.Select(r => UserPlayRecord.Create(osuId, mode, userInfo1.PlayCount, r)));
                }
                dbContext.UserPlayRecords.AddRange(toAdd);
                await dbContext.SaveChangesAsync();
                report.AddedPlayRecords = toAdd;
            }
            else
            {
                _logger.LogWarning("Concurrent error! Get different play_count in two queries ({userInfo1_PlayCount}, {userInfo2_PlayCount}). The user is {userInfo1.Name}({userInfo1.Id}); the mode is {mode}", userInfo1.PlayCount, userInfo2?.PlayCount, userInfo1.Name, userInfo1.Id, mode);
            }
            return report;
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

        public struct UpdateReport
        {
            public bool AddedSnapshot { get; set; }
            public bool UserNotExists { get; set; }
            public bool Inactive { get; set; }
            public required UserRecent[] UserRecents { get; set; }
            public required IList<UserPlayRecord> AddedPlayRecords { get; set; }
        }

        private static bool PropertyEquals<T>(T left, T right) => PropertyEquals(left, right, typeof(T));

        private static bool PropertyEquals(object? left, object? right, Type type)
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
