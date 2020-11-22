using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Utilities;
using Bleatingsheep.Osu;
using Bleatingsheep.Osu.ApiClient;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.Osu
{
#nullable enable
    public class QueryHelper
    {
        private readonly IOsuApiClient _osuApi;
        private readonly NewbieContext _newbieContext;

        public QueryHelper(IOsuApiClient osuApi, NewbieContext newbieContext)
        {
            _osuApi = osuApi;
            _newbieContext = newbieContext;
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
                return (true, history);
            }
            catch (Exception)
            {
                // TODO: logging
                return (false, default);
            }
        }

        private async ValueTask<(bool, UserInfo?, UserSnapshot?)> QueryInternal(int userId, Mode mode)
        {
            var userTask = _osuApi.GetUser(userId, mode);
            var historyTask = GetComparedData(userId, mode);
            var (historySuccess, history) = await historyTask.ConfigureAwait(false);
            try
            {
                var user = await userTask.ConfigureAwait(false);
                return (historySuccess, user, history);
            }
            catch (Exception)
            {
                // TODO: logging
                return (false, default, history);
            }
        }

        private async ValueTask<(bool, UserInfo?, UserSnapshot?)> QueryInternal(string userName, Mode mode)
        {
            try
            {
                var user = await _osuApi.GetUser(userName, mode).ConfigureAwait(false);
                if (user is null)
                    return (true, user, default);

                var (historySuccess, history) = await GetComparedData((int)user.Id, mode).ConfigureAwait(false);
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
                (true, null, _) => new Message("被办了。"),
                (_, UserInfo userInfo, var snapshot) => BuildMessage(userInfo, snapshot, desired_mode),
                (false, null, null) => new Message("查询失败。"),
                (false, null, var snapshot) => BuildMessage(snapshot.UserInfo, default, desired_mode) + new Message("\r\n查询 API 失败。"),
            };
        }

        private Message BuildMessage(UserInfo userInfo, UserSnapshot? snapshot, Mode? mode)
        {
            var compared = snapshot?.UserInfo;
            var text = $@"bleatingsheep的个人信息{(mode is null ? "" : "—" + mode.Value.GetShortModeString())}

{userInfo.Performance:#.##}pp 表现{IncrementUtility.FormatIncrement(userInfo.Performance - compared?.Performance)}
#{userInfo.Rank}{IncrementUtility.FormatIncrement(userInfo.Rank - compared?.Rank, '↓', '↑')}
{userInfo.CountryName} #{userInfo.CountryRank}{IncrementUtility.FormatIncrement(userInfo.CountryRank - compared?.CountryRank, '↓', '↑')}
{userInfo.RankedScore / 1_000_000:#,###}m Ranked谱面总分{IncrementUtility.FormatIncrement((userInfo.RankedScore - userInfo.RankedScore) / 1_000_000, "#,###")}
98.97% 准确率{IncrementUtility.FormatIncrementPercentage(userInfo.AccuracyFloat - compared?.AccuracyFloat)}
8040 游玩次数{IncrementUtility.FormatIncrement(userInfo.PlayCount - compared?.PlayCount)}
4,150,253 总命中次数{IncrementUtility.FormatIncrement(userInfo.TotalHits - compared?.TotalHits)}
253 小时 21 分钟 26 秒游玩时间{IncrementUtility.FormatIncrement(userInfo.PlayTime.TotalSeconds - compared?.PlayTime.TotalSeconds)}";
            return new Message(text);
        }
    }
#nullable restore
}
