using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core;
using Bleatingsheep.NewHydrant.Extentions;
using Bleatingsheep.Osu.PerformancePlus;
using Sisters.WudiLib.Posts;

namespace Bleatingsheep.NewHydrant.Osu
{
    [Function("pp_stars")]
    internal class PPBeatmapInfo : IMessageCommand
    {
        private static readonly PerformancePlusSpider s_spider = new PerformancePlusSpider();

        public IMessageCommand Create() => new PPBeatmapInfo();
        public async Task ProcessAsync(Message message, Sisters.WudiLib.HttpApiClient api, ExecutingInfo executingInfo)
        {
            long id = message.UserId;
            var exec = await executingInfo.Database.GetBindingIdAsync(id);
            if (!await exec.SendIfFailOrNoBindAsync(api, message.Endpoint)) return;

            var osuId = exec.Result.Value;
            var recent = (await executingInfo.OsuApi.GetRecentlyAsync(osuId, OsuMixedApi.Mode.Standard, 1)).FirstOrDefault();
            if (recent == null)
            {
                await api.SendMessageAsync(message.Endpoint, "没打图！");
                return;
            }

            var reply = new List<string> { "本功能免费试用，将来可能作为 Chart 奖励。" };
            try
            {
                var ppBeatmap = await s_spider.GetBeatmapPlusAsync(recent.BeatmapId);
                if (ppBeatmap == null)
                {
                    reply.Add("PP+ 可能更改了网页布局，无法获取到谱面信息。");
                    return;
                }
                reply.Add($"https://syrin.me/pp+/b/{ppBeatmap.Id}/");
                reply.Add("Stars: " + ppBeatmap.Stars);
                reply.Add("Aim (Jump): " + ppBeatmap.AimJump);
                reply.Add("Aim (Flow): " + ppBeatmap.AimFlow);
                reply.Add("Precision: " + ppBeatmap.Precision);
                reply.Add("Speed: " + ppBeatmap.Speed);
                reply.Add("Stamina: " + ppBeatmap.Stamina);
                reply.Add("Accuracy: " + ppBeatmap.Accuracy);
                reply.Add("数据来自 PP+。");
            }
            catch (ExceptionPlus)
            {
                reply.Add("访问 PP+ 网站失败。");
            }
            finally
            {
                await api.SendMessageAsync(message.Endpoint, string.Join("\r\n", reply));
            }
        }

        public bool ShouldResponse(Message message)
            => message.Content.IsPlaintext
            && message.Content.Text.Equals(" pp", StringComparison.OrdinalIgnoreCase);
    }
}
