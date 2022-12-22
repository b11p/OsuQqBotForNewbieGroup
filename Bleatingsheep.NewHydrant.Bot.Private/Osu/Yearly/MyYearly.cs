using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
    public partial class MyYearly : IMessageCommand
    {
        private readonly IDataProvider _dataProvider;
        private readonly IOsuApiClient _osuApiClient;
        private readonly NewbieContext _newbieContext;
        private Mode _mode = Mode.Standard;
        private readonly TimeSpan _timeZone = TimeSpan.FromHours(8);

        private IReadOnlyList<UserPlayRecord> _userPlayRecords = Array.Empty<UserPlayRecord>();

        public MyYearly(IDataProvider dataProvider, IOsuApiClient osuApiClient, NewbieContext newbieContext)
        {
            _dataProvider = dataProvider;
            _osuApiClient = osuApiClient;
            _newbieContext = newbieContext;
        }

        private string ModeString { get; set; }

        public async Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            // check binding
            DateTimeOffset now = DateTimeOffset.UtcNow;
            DateTimeOffset start = now.AddYears(-1);
            int? osuId = await _dataProvider.GetOsuIdAsync(context.UserId).ConfigureAwait(false);
            if (osuId == null)
            {
                _ = await api.SendMessageAsync(context.Endpoint, "未绑定").ConfigureAwait(false);
                return;
            }

            // apply mode
            try
            {
                _mode = Bleatingsheep.Osu.ModeExtensions.Parse(ModeString);
            }
            catch
            {
                await api.SendMessageAsync(context.Endpoint, $"未知游戏模式{ModeString}。将生成 std 模式的报告。").ConfigureAwait(false);
            }

            // retrieve data from local snapshots.
            UserSnapshot? snap = await _newbieContext.UserSnapshots
                .Where(s => s.UserId == osuId && s.Mode == _mode && s.Date > start)
                .OrderBy(s => s.Date)
                .FirstOrDefaultAsync().ConfigureAwait(false);
            if (snap is null)
            {
                _ = await api.SendMessageAsync(context.Endpoint, "没有找到数据").ConfigureAwait(false);
                return;
            }

            // get current state
            UserInfo? userInfo = await _osuApiClient.GetUser(osuId.Value, _mode).ConfigureAwait(false);
            if (userInfo is null)
            {
                // user may be banned
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
            _ = await api.SendMessageAsync(context.Endpoint, $"当前模式：{_mode}数据完整度：{playList.Count} of {currentPC - startPC}。功能制作中。正在生成报告，请稍候。").ConfigureAwait(false);
            if (playList.Count == 0)
                return;

            // favorite mapper
            var playedBeatmaps = playList.Select(r => r.Record.BeatmapId).Distinct().ToHashSet();
            var cachedBeatmapInfo = await _newbieContext.BeatmapInfoCache.Where(e => e.Mode == _mode && playedBeatmaps.Contains(e.BeatmapId)).ToListAsync().ConfigureAwait(false);
            var remainingBeatmaps = playedBeatmaps.Except(cachedBeatmapInfo.Select(e => e.BeatmapInfo.Id));
            var beatmapInfoList = cachedBeatmapInfo.ConvertAll(e => e.BeatmapInfo);
            string? favoriteMapperName = default;
            int favoriteMapperPlayCount = default;
            var mapperTask = Task.Run(async () =>
            {
                foreach (var beatmapId in remainingBeatmaps)
                {
                    var current = await _dataProvider.GetBeatmapInfoAsync(beatmapId, _mode).ConfigureAwait(false);
                    if (current != null)
                    {
                        beatmapInfoList.Add(current);
                    }
                }
                var beatmapToCreatorDic = beatmapInfoList.ToDictionary(i => i.Id, i => i.CreatorId);
                var favoriteMapperId = playedBeatmaps.GroupBy(bid => beatmapToCreatorDic.GetValueOrDefault(bid)).OrderByDescending(g => g.Count()).SkipWhile(g => g.Key == 0).FirstOrDefault()?.Key;
                if (favoriteMapperId != null)
                {
                    var favoriteMapperBeatmaps = beatmapInfoList.Where(b => b.CreatorId == favoriteMapperId).ToList();
                    favoriteMapperName = favoriteMapperBeatmaps.OrderByDescending(b => b.ApprovedDate).FirstOrDefault()?.Creator;
                    var favoriteMapperBeatmapIds = favoriteMapperBeatmaps.Select(b => b.Id).ToHashSet();
                    favoriteMapperPlayCount = playList.Count(r => favoriteMapperBeatmapIds.Contains(r.Record.BeatmapId));
                }
            });
            try
            {
                // favorite mapper
                await mapperTask.ConfigureAwait(false);
            }
            catch
            {
            }

            // assign data to fields.
            _userPlayRecords = playList;
            {
                // days played
                (int days, int totalDays) = GetPlayedDays();
                _ = await api.SendMessageAsync(context.Endpoint, $"你在过去一年中有 {days} 天打了图。").ConfigureAwait(false);
            }
            {
                // most played
                (int bid, int count, BeatmapInfo? beatmap) = await GetMostPlayedBeatmapAsync().ConfigureAwait(false);
                _ = await api.SendMessageAsync(context.Endpoint, $"你最常打的一张图是 {bid}，打了 {count} 次。" +
                    $"{beatmap}").ConfigureAwait(false);
            }
            {
                if (favoriteMapperName != null)
                {
                    _ = await api.SendMessageAsync(context.Endpoint, $"你最喜欢的 mapper 是 {favoriteMapperName}，打了她/他的图 {favoriteMapperPlayCount} 次。").ConfigureAwait(false);
                }
            }
            {
                // most playing hour
                var mostPlayingHour = GetMostPlayingHours();
                _ = await api.SendMessageAsync(context.Endpoint, $"你最常在 {mostPlayingHour}-{mostPlayingHour + 1} 时打图。").ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Get the number of days with play records.
        /// </summary>
        /// <returns></returns>
        private (int days, int totalDays) GetPlayedDays()
        {
            List<DateTime> days = _userPlayRecords.Select(r => r.Record.Date.Date).Distinct().ToList();
            return (days.Count, 365);
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

        [GeneratedRegex("^我的年度(?:屙屎|osu[!！]?)\\s*(?:[,，]\\s*(?<mode>.+?)\\s*)?$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
        private static partial Regex MatchingRegex();
        private static readonly Regex s_regex = MatchingRegex();

        public bool ShouldResponse(MessageContext context)
        {
            if (!context.Content.TryGetPlainText(out string text))
            {
                return false;
            }
            var match = s_regex.Match(text.Trim());
            if (!match.Success)
            {
                return false;
            }
            var modeGroup = match.Groups["mode"];
            ModeString = modeGroup.Value;
            return true;
        }
    }
#nullable restore
}
