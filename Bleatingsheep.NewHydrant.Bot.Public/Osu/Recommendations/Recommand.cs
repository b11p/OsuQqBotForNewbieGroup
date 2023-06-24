using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.Osu;
using Bleatingsheep.Osu.ApiClient;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Sisters.WudiLib;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.Osu.Recommendations
{
#nullable enable
    [Component("Recommand")]
    public class Recommand : IMessageCommand
    {
        private readonly IOsuApiClient _osuApiClient;
        private readonly NewbieContext _newbieContext;

        public Recommand(IOsuApiClient osuApiClient, NewbieContext newbieContext)
        {
            _osuApiClient = osuApiClient;
            _newbieContext = newbieContext;
        }

        public async Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            var bi = await _newbieContext.Bindings.Where(b => b.UserId == context.UserId).FirstOrDefaultAsync().ConfigureAwait(false);
            if (bi == null)
                return;
            var osuId = bi.OsuId;
            var best = await _osuApiClient.GetUserBest(osuId).ConfigureAwait(false);
            var sb = new StringBuilder();
            foreach (var b in best.Take(4))
            {
                sb.Append("根据您的 BP b/").Append(b.BeatmapId).Append(GetModsString(b.EnabledMods)).AppendLine(" 推荐：");
                var id = RecommendationBeatmapId.Create(b, Mode.Standard);
                var rec = await _newbieContext.Recommendations
                    .Where(r => r.Left == id)
                    .OrderByDescending(r => r.RecommendationDegree)
                    .Take(4)
                    .ToListAsync().ConfigureAwait(false);

                _ = rec.Aggregate(sb, (sb, r) =>
                    sb.Append("b/").Append(r.Recommendation.BeatmapId).Append(GetModsString(r.Recommendation.ValidMods)).AppendLine($"({r.Performance} PP)"));
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

        public bool ShouldResponse(MessageContext context)
        {
            return context.Content.Text == "打什么图";
        }
    }
#nullable restore
}
