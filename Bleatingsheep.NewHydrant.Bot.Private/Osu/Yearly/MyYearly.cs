using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Data;
using Bleatingsheep.Osu;
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
        private readonly TimeSpan _timeZone = TimeSpan.FromHours(8);

        private Mode _mode = Mode.Standard;
        private IReadOnlyList<UserPlayRecord> _userPlayRecords = default!;
        private IReadOnlyDictionary<int, BeatmapInfo> _beatmapInfoDict = default!;
        private bool _hasError = false;

        public MyYearly(IDataProvider dataProvider, IOsuApiClient osuApiClient, NewbieContext newbieContext)
        {
            _dataProvider = dataProvider;
            _osuApiClient = osuApiClient;
            _newbieContext = newbieContext;
        }

        private string ModeString { get; set; } = default!;

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
                if (!string.IsNullOrWhiteSpace(ModeString))
                {
                    _mode = ModeExtensions.Parse(ModeString);
                }
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
            UserInfo? userInfo = default;
            try
            {
                userInfo = await _osuApiClient.GetUser(osuId.Value, _mode).ConfigureAwait(false);
            }
            catch
            {
                // ignore, use local data
                _hasError = true;
            }
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
            _ = await api.SendMessageAsync(context.Endpoint, $"{userInfo.Name}。当前模式：{_mode}，数据始于{snap.Date.ToOffset(_timeZone):M月d日}，完整度：{playList.Count}/{currentPC - startPC}。功能制作中。正在生成报告，请稍候。").ConfigureAwait(false);
            if (playList.Count == 0)
            {
                await api.SendMessageAsync(context.Endpoint, "你在过去一年没有玩儿过 osu!，或无数据记录。").ConfigureAwait(false);
                return;
            }

            // beatmap info
            var playedBeatmaps = playList.Select(r => r.Record.BeatmapId).Distinct().ToHashSet();
            var cachedBeatmapInfo = await _newbieContext.BeatmapInfoCache.Where(e => e.Mode == _mode && playedBeatmaps.Contains(e.BeatmapId)).ToListAsync().ConfigureAwait(false);
            var noCacheBeatmaps = playedBeatmaps.Except(cachedBeatmapInfo.Select(e => e.BeatmapInfo.Id));
            var beatmapInfoList = cachedBeatmapInfo.ConvertAll(e => e.BeatmapInfo);
            foreach (var beatmapId in noCacheBeatmaps)
            {
                try
                {
                    var current = await _dataProvider.GetBeatmapInfoAsync(beatmapId, _mode).ConfigureAwait(false);
                    if (current != null)
                    {
                        beatmapInfoList.Add(current);
                    }
                }
                catch
                {
                    _hasError = true;
                }
            }
            _beatmapInfoDict = beatmapInfoList.ToDictionary(bi => bi.Id);

            // assign data to fields.
            _userPlayRecords = playList;
            var sb = new StringBuilder();
            if (_hasError)
            {
                sb.AppendLine("由于请求量过高，有错误发生，数据可能不准确。");
            }
            {
                // days played
                (int days, int totalDays) = GetPlayedDays();
                sb.Append($"你在过去一年中有 {days} 天打了图，合计 {userInfo.PlayCount - snap.UserInfo.PlayCount} 次，{userInfo.TotalHits - snap.UserInfo.TotalHits} TTH，{(userInfo.PlayTime - snap.UserInfo.PlayTime).TotalHours:#.##} 小时。");
                sb.AppendLine($"增长了 {userInfo.Performance - snap.UserInfo.Performance:#.##}PP。");
            }
            {
                // most played
                (int bid, int count, BeatmapInfo? beatmap) = GetMostPlayedBeatmap();
                sb.AppendLine($"你最常打的一张图是 {bid}，打了 {count} 次。{beatmap}");
            }
            {
                // mods
                var (mods, count) = GetFavoriteMods();
                var modsString = mods.Display();
                if (string.IsNullOrEmpty(modsString))
                {
                    modsString = "None";
                }
                sb.AppendLine($"你最喜欢的 mods 是 {modsString}，贡献了你 {(double)count / _userPlayRecords.Count:P0} 的游玩次数。");
            }
            {
                (string? favoriteMapperName, int favoriteMapperPlayCount) = GetFavoriteMapper();
                if (favoriteMapperName != null)
                {
                    sb.AppendLine($"你最喜欢的 mapper 是 {favoriteMapperName}，打了她/他的图 {favoriteMapperPlayCount} 次。");
                }
            }
            {
                // most playing hour
                var mostPlayingHour = GetMostPlayingHours();
                sb.AppendLine($"你最常在 {mostPlayingHour}-{mostPlayingHour + 1} 时打图。");
            }
            {
                // most played beatmap of the day
                var (bid, date, count, fc) = GetMostPlayedBeatmapOfDay();
                string fcString = fc == true
                    ? "全连了，真不容易。"
                    : fc == false
                    ? "都没全连，真菜。"
                    : string.Empty;
                var beatmapInfo = _beatmapInfoDict.GetValueOrDefault(bid);
                sb.AppendLine($"{date.ToShortDateString()}，你把 {bid} 打了 {count} 次。{fcString}{beatmapInfo}");
            }
            {
                // longest continuous play
                var (start1, end, pc, tth) = GetLongestContinuousPlay(out var periods);
                if (start1 != end)
                {
                    sb.AppendLine($"{start1:M-d H:mm} 到 {end:M-d H:mm}，你连续打了 {pc} 次，是你连续游玩时间最长的一次，你一定玩儿得热血沸腾。");
                }
                if (periods.Count > 0)
                {
                    var mostNight = periods.OrderByDescending(t => t.end.AddHours(-5).ToOffset(_timeZone).TimeOfDay).First();
                    var dtoff = mostNight.end.ToOffset(_timeZone);
                    var date = dtoff.Date;
                    var time = dtoff.TimeOfDay;
                    // var comment = time.Hours switch
                    // {
                    //     < 2 => "要注意休息。",
                    //     < 5 => "osu! 陪你度过不眠夜。",
                    //     < 18 => "大好的晚上不能浪费在 osu! 上。",
                    //     _ => "除了 osu!，你还有人生，健康作息很重要。",
                    // };
                    sb.AppendLine($"{date.ToShortDateString()}，你{time.Hours}点{time.Minutes}分还在打 osu!，是最晚的一次。");
                }
            }
            sb.Append($"{userInfo.Name} 的年度 osu! 记录。");
            await api.SendMessageAsync(context.Endpoint, sb.ToString()).ConfigureAwait(false);
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

        private (int bid, int count, BeatmapInfo? beatmapInfo) GetMostPlayedBeatmap()
        {
            IGrouping<int, UserPlayRecord>? mostPlayed = _userPlayRecords
                .GroupBy(r => r.Record.BeatmapId)
                .OrderByDescending(g => g.Count())
                .First();
            (int bid, int count) = (mostPlayed.Key, mostPlayed.Count());
            BeatmapInfo? beatmap = _beatmapInfoDict.GetValueOrDefault(bid);
            return (bid, count, beatmap);
        }

        private (Mods mods, int count) GetFavoriteMods()
        {
            var favModsGroup = _userPlayRecords.GroupBy(r => r.Record.EnabledMods).OrderByDescending(g => g.Count()).First();
            return (favModsGroup.Key, favModsGroup.Count());
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

        private (string? favoriteMapperName, int favoriteMapperPlayCount) GetFavoriteMapper()
        {
            var playedBeatmaps = _userPlayRecords.Select(r => r.Record.BeatmapId).Distinct().ToHashSet();
            var favoriteMapperId = playedBeatmaps.GroupBy(bid => _beatmapInfoDict.GetValueOrDefault(bid)?.CreatorId).OrderByDescending(g => g.Count()).FirstOrDefault(g => g.Key != default)?.Key;
            if (favoriteMapperId != null)
            {
                var favoriteMapperBeatmaps = _beatmapInfoDict.Values.Where(b => b.CreatorId == favoriteMapperId).ToList();
                var favoriteMapperName = favoriteMapperBeatmaps.OrderByDescending(b => b.ApprovedDate).FirstOrDefault()?.Creator;
                var favoriteMapperBeatmapIds = favoriteMapperBeatmaps.Select(b => b.Id).ToHashSet();
                var favoriteMapperPlayCount = _userPlayRecords.Count(r => favoriteMapperBeatmapIds.Contains(r.Record.BeatmapId));
                return (favoriteMapperName, favoriteMapperPlayCount);
            }
            return default;
        }

        private (int beatmapId, DateTime date, int count, bool? fullCombo) GetMostPlayedBeatmapOfDay()
        {
            var mostPlayedOfTheDay = _userPlayRecords.GroupBy(r =>
            {
                var adjustedDate = new DateTimeOffset(r.Record.Date).ToOffset(_timeZone).AddHours(-5).Date;
                return (r.Record.BeatmapId, adjustedDate);
            }).OrderByDescending(g => g.Count()).First();
            var (bid, date) = mostPlayedOfTheDay.Key;
            var beatmapInfo = _beatmapInfoDict.GetValueOrDefault(bid);
            bool? fullCombo = default;
            if (beatmapInfo != null)
            {
                var maxCombo = beatmapInfo.MaxCombo;
                fullCombo = mostPlayedOfTheDay.Any(r => r.Record.CountMiss == 0 && r.Record.Count100 + r.Record.Count50 > maxCombo - r.Record.MaxCombo);
            }
            return (bid, date, mostPlayedOfTheDay.Count(), fullCombo);
        }

        private (DateTimeOffset start, DateTimeOffset end, int pc, int tth) GetLongestContinuousPlay(out List<(DateTimeOffset start, DateTimeOffset end, int pc, int tth)> periods)
        {
            periods = new List<(DateTimeOffset start, DateTimeOffset end, int pc, int tth)>();
            var start = _userPlayRecords[0].Record.Date;
            int pc = 0;
            int tth = 0;
            var last = _userPlayRecords[0].Record.Date;
            foreach (var r in _userPlayRecords)
            {
                if (last.AddHours(2) < r.Record.Date)
                {
                    if (start != last)
                    {
                        periods.Add((start, last, pc, tth));
                    }
                    start = r.Record.Date;
                    pc = 0;
                    tth = 0;
                }
                pc++;
                tth += r.Record.Count300 + r.Record.Count100 + r.Record.Count50;
                last = r.Record.Date;
            }
            if (start != last)
            {
                periods.Add((start, last, pc, tth));
            }
            return periods.Count == 0 ? default : periods.MaxBy(t => t.end - t.start);
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
