using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Data;
using Bleatingsheep.Osu.ApiClient;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Sisters.WudiLib;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;
using Mode = Bleatingsheep.Osu.Mode;

namespace Bleatingsheep.NewHydrant.Osu.Yearly
{
#nullable enable
    [Component("MyYearly")]
    public class MyYearly : IMessageCommand
    {
        private readonly IDataProvider _dataProvider;
        private readonly IOsuApiClient _osuApiClient;
        private readonly NewbieContext _newbieContext;
        private readonly Mode _mode = Mode.Standard;
        private readonly TimeSpan _timeZone = TimeSpan.FromHours(8);

        private IReadOnlyList<UserPlayRecord> _userPlayRecords = Array.Empty<UserPlayRecord>();

        public MyYearly(IDataProvider dataProvider, IOsuApiClient osuApiClient, NewbieContext newbieContext)
        {
            _dataProvider = dataProvider;
            _osuApiClient = osuApiClient;
            _newbieContext = newbieContext;
        }

        public async Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            DateTimeOffset start = now.AddYears(-1);
            int? osuId = await _dataProvider.GetOsuIdAsync(context.UserId).ConfigureAwait(false);
            if (osuId == null)
            {
                _ = await api.SendMessageAsync(context.Endpoint, "未绑定").ConfigureAwait(false);
                return;
            }
            UserSnapshot? snap = await _newbieContext.UserSnapshots
                .Where(s => s.UserId == osuId && s.Mode == _mode && s.Date > start)
                .OrderBy(s => s.Date)
                .FirstOrDefaultAsync().ConfigureAwait(false);
            if (snap is null)
            {
                _ = await api.SendMessageAsync(context.Endpoint, "没有找到数据").ConfigureAwait(false);
                return;
            }
            UserInfo? userInfo = await _osuApiClient.GetUser(osuId.Value, _mode).ConfigureAwait(false);
            if (userInfo is null)
            {
                userInfo = (await _newbieContext.UserSnapshots
                    .Where(s => s.UserId == osuId && s.Mode == _mode)
                    .OrderByDescending(s => s.Date)
                    .FirstAsync().ConfigureAwait(false)).UserInfo;
            }
            int startPC = snap.UserInfo.PlayCount;
            int currentPC = userInfo.PlayCount;
            List<UserPlayRecord> playList = await _newbieContext.UserPlayRecords
                .Where(r => r.UserId == osuId && r.Mode == _mode && r.PlayNumber > startPC)
                .OrderBy(r => r.PlayNumber)
                .ToListAsync().ConfigureAwait(false);
            _ = await api.SendMessageAsync(context.Endpoint, $"数据完整度：{playList.Count} of {currentPC - startPC}。功能制作中。").ConfigureAwait(false);
            if (playList.Count == 0)
                return;

            // assign data to fields.
            _userPlayRecords = playList;
            {
                // most played
                (int bid, int count, BeatmapInfo? beatmap) = await GetMostPlayedBeatmapAsync().ConfigureAwait(false);
                _ = await api.SendMessageAsync(context.Endpoint, $"你最常打的一张图是 {bid}，打了 {count} 次。" +
                    $"{beatmap}").ConfigureAwait(false);
            }
            {
                // most playing hour
                var mostPlayingHour = GetMostPlayingHours();
                _ = await api.SendMessageAsync(context.Endpoint, $"你最常在 {mostPlayingHour}-{mostPlayingHour + 1} 时打图。").ConfigureAwait(false);
            }
        }

        public bool ShouldResponse(MessageContext context)
        {
            return context.Content.TryGetPlainText(out string text)
                && text == "我的年度屙屎";
        }

        private async Task<(int bid, int count, BeatmapInfo? beatmapInfo)> GetMostPlayedBeatmapAsync()
        {
            IGrouping<int, UserPlayRecord>? mostPlayed = _userPlayRecords
                .GroupBy(r => r.Record.BeatmapId)
                .OrderByDescending(g => g.Count())
                .First();
            (int bid, int count) = (mostPlayed.Key, mostPlayed.Count());
            BeatmapInfo? beatmap = await _osuApiClient.GetBeatmap(bid, _mode).ConfigureAwait(false);
            return (bid, count, beatmap);
        }

        private int GetMostPlayingHours()
        {
            int mostPlayingHour = _userPlayRecords
                .GroupBy(r => new DateTimeOffset(r.Record.Date).ToOffset(_timeZone).Hour)
                .OrderByDescending(_g => _g.Count())
                .First()
                .Key;
            return mostPlayingHour;
        }
    }
#nullable restore
}
