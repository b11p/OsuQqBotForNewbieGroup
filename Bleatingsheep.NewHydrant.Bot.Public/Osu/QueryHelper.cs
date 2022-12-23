using System;
using System.Linq;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Data;
using Bleatingsheep.NewHydrant.Utilities;
using Bleatingsheep.Osu;
using Bleatingsheep.Osu.ApiClient;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.Osu
{
#nullable enable
    public class QueryHelper
    {
        private readonly IOsuApiClient _osuApi;
        private readonly NewbieContext _newbieContext;
        private readonly DataMaintainer _dataMaintainer;
        private readonly ILogger<QueryHelper> _logger;

        public QueryHelper(IOsuApiClient osuApi, NewbieContext newbieContext, DataMaintainer dataMaintainer, ILogger<QueryHelper> logger)
        {
            _osuApi = osuApi;
            _newbieContext = newbieContext;
            _dataMaintainer = dataMaintainer;
            _logger = logger;
        }

        private async ValueTask<(bool, UserSnapshot?)> GetComparedData(int userId, Mode mode)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            var comparedDate = now.AddHours(-36);
            try
            {
                var snapshots = await _newbieContext.UserSnapshots
                    .Where(s => s.UserId == userId && s.Mode == mode && s.Date > comparedDate)
                    .ToListAsync().ConfigureAwait(false);
                var history = snapshots
                    .OrderBy(s => Utilities.DateUtility.GetError(now - TimeSpan.FromHours(24), s.Date))
                    .FirstOrDefault();
                if (history == null)
                {
                    // 当用户比较数据不存在时，强制更新
                    _ = _dataMaintainer.UpdateAsync(userId, mode);
                }
                return (true, history);
            }
            catch (Exception)
            {
                // TODO: logging
                return (false, default);
            }
        }

        private async ValueTask<(bool succeeded, UserInfo?, UserSnapshot?)> QueryInternal(int userId, Mode mode)
        {
            var userTask = _osuApi.GetUser(userId, mode);
            var historyTask = GetComparedData(userId, mode);
            var (historySuccess, history) = await historyTask.ConfigureAwait(false);
            try
            {
                var user = await userTask.ConfigureAwait(false);
                if (historySuccess && user?.Performance is not > 0 && history == null)
                {
                    // the user may be inactive
                    try
                    {
                        history = await _newbieContext.UserSnapshots.AsNoTracking().Where(s => s.UserId == userId && s.Mode == mode).OrderByDescending(s => s.Date).FirstOrDefaultAsync().ConfigureAwait(false);
                    }
                    catch
                    {
                        historySuccess = false;
                    }
                }
                return (historySuccess, user, history);
            }
            catch (Exception)
            {
                // TODO: logging
                return (false, default, history);
            }
        }

        private async ValueTask<(bool succeeded, UserInfo?, UserSnapshot?)> QueryInternal(string userName, Mode mode)
        {
            try
            {
                var user = await _osuApi.GetUser(userName, mode).ConfigureAwait(false);
                if (user is null)
                    return (true, user, default);

                var (historySuccess, history) = await GetComparedData((int)user.Id, mode).ConfigureAwait(false);
                if (historySuccess && user.Performance == 0 && history == null)
                {
                    // the user may be inactive
                    try
                    {
                        history = await _newbieContext.UserSnapshots.AsNoTracking().Where(s => s.UserId == user.Id && s.Mode == mode).OrderByDescending(s => s.Date).FirstOrDefaultAsync().ConfigureAwait(false);
                    }
                    catch
                    {
                        historySuccess = false;
                    }
                }
                return (historySuccess, user, history);
            }
            catch (Exception)
            {
                // TODO: logging
                return (false, default, default);
            }
        }

        public async ValueTask<Message> QueryByUserId(int userId, Mode? desired_mode)
        {
            Mode mode = desired_mode ?? Mode.Standard;
            return await QueryInternal(userId, mode).ConfigureAwait(false) switch
            {
                (true, null, UserSnapshot snapshot) => BuildMessageFromInactive(null, snapshot, desired_mode) + new Message("\r\n已被 ban。"),
                (true, null, _) => new Message("被办了。"),
                (_, UserInfo { Performance: 0.0 } userInfo, UserSnapshot { UserInfo.Performance: > 0.0 } snapshot) => BuildMessageFromInactive(userInfo, snapshot, desired_mode),
                (_, UserInfo userInfo, var snapshot) => BuildMessage(userInfo, snapshot, desired_mode),
                (false, null, null) => new Message("查询失败。"),
                (false, null, var snapshot) => BuildMessage(snapshot.UserInfo, default, desired_mode) + new Message("\r\n查询 API 失败。"),
            };
        }

        public async ValueTask<Message> QueryByUserName(string userName, Mode? desired_mode)
        {
            Mode mode = desired_mode ?? Mode.Standard;
            return await QueryInternal(userName, mode).ConfigureAwait(false) switch
            {
                (true, null, UserSnapshot snapshot) => BuildMessageFromInactive(null, snapshot, desired_mode) + new Message("\r\n已被 ban。"),
                (true, null, _) => new Message("没这个人。"),
                (_, UserInfo { Performance: 0.0 } userInfo, UserSnapshot { UserInfo.Performance: > 0.0 } snapshot) => BuildMessageFromInactive(userInfo, snapshot, desired_mode),
                (_, UserInfo userInfo, var snapshot) => BuildMessage(userInfo, snapshot, desired_mode),
                (false, null, null) => new Message("查询失败。"),
                (false, null, var snapshot) => BuildMessage(snapshot.UserInfo, default, desired_mode) + new Message("\r\n查询 API 失败。"),
            };
        }

        private static Message BuildMessage(UserInfo userInfo, UserSnapshot? snapshot, Mode? mode)
        {
            var compared = snapshot?.UserInfo;
            var text = $@"{userInfo.Name}的个人信息{(mode is null ? "" : "—" + mode.Value.GetShortModeString())}

{userInfo.Performance:0.##}pp 表现{IncrementUtility.FormatIncrement(userInfo.Performance - compared?.Performance)}
#{userInfo.Rank}{IncrementUtility.FormatIncrement(userInfo.Rank - compared?.Rank, '↓', '↑')}
{userInfo.CountryName} #{userInfo.CountryRank}{IncrementUtility.FormatIncrement(userInfo.CountryRank - compared?.CountryRank, '↓', '↑')}
{userInfo.RankedScore / 1_000_000.0:#,##0}m Ranked谱面总分{IncrementUtility.FormatIncrement((userInfo.RankedScore - userInfo.RankedScore) / 1_000_000.0, "#,###")}
{userInfo.AccuracyFloat:0.##%} 准确率{IncrementUtility.FormatIncrementPercentage(userInfo.AccuracyFloat - compared?.AccuracyFloat)}
{userInfo.PlayCount} 游玩次数{IncrementUtility.FormatIncrement(userInfo.PlayCount - compared?.PlayCount)}
{userInfo.TotalHits:#,##0} 总命中次数{IncrementUtility.FormatIncrement(userInfo.TotalHits - compared?.TotalHits, "#,###")}
{userInfo.PlayTime.Days * 24 + userInfo.PlayTime.Hours} 小时 {userInfo.PlayTime.Minutes} 分钟 {userInfo.PlayTime.Seconds} 秒游玩时间{IncrementUtility.FormatIncrement(userInfo.PlayTime.TotalSeconds - compared?.PlayTime.TotalSeconds, "#,###")}";
            return new Message(text);
        }

        private static Message BuildMessageFromInactive(UserInfo? userInfo, UserSnapshot snapshot, Mode? mode)
        {
            userInfo ??= snapshot.UserInfo;
            var text = $@"{userInfo.Name}的个人信息{(mode is null ? "" : "—" + mode.Value.GetShortModeString())}

{snapshot.UserInfo.Performance:0.##}pp 表现
{userInfo.RankedScore / 1_000_000.0:#,##0}m Ranked谱面总分
{snapshot.UserInfo.AccuracyFloat:0.##%} 准确率
{userInfo.PlayCount} 游玩次数
{userInfo.TotalHits:#,##0} 总命中次数
{userInfo.PlayTime.Days * 24 + userInfo.PlayTime.Hours} 小时 {userInfo.PlayTime.Minutes} 分钟 {userInfo.PlayTime.Seconds} 秒游玩时间
用户不活跃";
            return new Message(text);
        }
    }
#nullable restore
}
