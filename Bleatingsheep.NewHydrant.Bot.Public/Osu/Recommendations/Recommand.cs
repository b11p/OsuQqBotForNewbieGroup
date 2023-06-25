using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.Osu;
using Bleatingsheep.Osu.ApiClient;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Sisters.WudiLib;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.Osu.Recommendations
{
#nullable enable
    [Component("Recommand")]
    public partial class Recommand : IMessageCommand
    {
        private readonly IOsuApiClient _osuApiClient;
        private readonly NewbieContext _newbieContext;

        public Recommand(IOsuApiClient osuApiClient, NewbieContext newbieContext)
        {
            _osuApiClient = osuApiClient;
            _newbieContext = newbieContext;
        }

        private Mode _mode = Mode.Standard;

        public async Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            var bi = await _newbieContext.Bindings.Where(b => b.UserId == context.UserId).FirstOrDefaultAsync().ConfigureAwait(false);
            if (bi == null)
                return;
            var osuId = bi.OsuId;
            var best = await _osuApiClient.GetUserBest(osuId, _mode).ConfigureAwait(false);
            if (best?.Length is not > 0)
            {
                await api.SendMessageAsync(context.Endpoint, "你并没有 BP 哦，所以无法找到可以推荐的图。").ConfigureAwait(false);
                return;
            }

            var currentPP = best.Select(b => new { b.BeatmapId, b.Performance }).ToList();

            var userName = _newbieContext.UserSnapshots
                .AsNoTracking()
                .Where(s => s.UserId == osuId && s.Mode == _mode)
                .OrderByDescending(s => s.Date)
                .Select(s => s.UserInfo.Name)
                .FirstOrDefault();
            userName ??= $"u/{osuId}";

            var sb = new StringBuilder();
            sb.AppendLine($"根据 BP 关联度，在 {_mode.GetShortModeString()} 模式给 {userName} 推荐的图如下：");
            foreach (var b in best.Take(4))
            {
                var id = RecommendationBeatmapId.Create(b, _mode);
                var rec = await
                    (from r in _newbieContext.Recommendations.Where(r_ => r_.Mode == _mode && r_.Left == id).AsAsyncEnumerable()
                     join c in currentPP.ToAsyncEnumerable() on r.Recommendation.BeatmapId equals c.BeatmapId into current
                     from c in current.DefaultIfEmpty()
                     where c == null || c.Performance < r.Performance * 0.99
                     orderby r.RecommendationDegree descending
                     select r)
                    .Take(4)
                    .ToListAsync();
                if (rec.Count > 0)
                {
                    sb.Append("根据您的 BP b/").Append(b.BeatmapId).Append(GetModsString(b.EnabledMods)).AppendLine(" 推荐：");
                    _ = rec.Aggregate(sb, (sb, r) =>
                        sb.Append("b/").Append(r.Recommendation.BeatmapId).Append(GetModsString(r.Recommendation.ValidMods)).AppendLine(GetPerformanceString(r, best)));
                }
                else
                {
                    sb.AppendLine($"没有找到与 b/{b.BeatmapId}{GetModsString(b.EnabledMods)} 相关的图，可能是你太强了，别人都打不出这个 BP。");
                }
            }
            await api.SendMessageAsync(context.Endpoint, sb.ToString()).ConfigureAwait(false);
        }

        public static string GetModsString(Mods mods)
        {
            var s = mods.Display();
            return string.IsNullOrEmpty(s)
                ? string.Empty
                : " + " + s;
        }

        public static string GetPerformanceString(RecommendationEntry recommendation, IEnumerable<UserBest> bpList)
        {
            double performance = recommendation.Performance;
            var currentPP = bpList.FirstOrDefault(b => b.BeatmapId == recommendation.Recommendation.BeatmapId)?.Performance;
            return double.IsNaN(performance)
                ? string.Empty
                : $" ({performance:0} PP{(currentPP != null ? $", 当前 {currentPP}" : string.Empty)})";
        }

        public bool ShouldResponse(MessageContext context)
        {
            var regex = MatchRegex();
            var match = regex.Match(context.Content.Text);
            if (!match.Success)
            {
                return false;
            }
            var modeString = match.Groups[1].Value;
            if (!string.IsNullOrEmpty(modeString))
            {
                try
                {
                    _mode = ModeExtensions.Parse(modeString);
                }
                catch { }
            }
            return true;
        }

        [GeneratedRegex("^\\s*打什么图\\s*[,，]?\\s*(.*?)\\s*$", RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled)]
        private static partial Regex MatchRegex();
    }
#nullable restore
}
