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
    internal class PPBeatmapInfo : OsuFunction, IMessageCommand
    {
        private static readonly PerformancePlusSpider s_spider = new PerformancePlusSpider();

        public IMessageCommand Create() => new PPBeatmapInfo();
        public async Task ProcessAsync(Message message, Sisters.WudiLib.HttpApiClient api, ExecutingInfo executingInfo)
        {
            long id = message.UserId;
            var (networkSuccess, osuResult) = await executingInfo.Data.GetBindingIdAsync(id);
            ExecutingException.Ensure(networkSuccess, "无法查询绑定账号。");
            ExecutingException.Ensure(osuResult != null, "未绑定 osu! 游戏账号。");

            var osuId = osuResult.Value;
            var recent = (await OsuApi.GetRecentlyAsync(osuId, OsuMixedApi.Mode.Standard, 1)).FirstOrDefault();
            if (recent == null)
            {
                await api.SendMessageAsync(message.Endpoint, "没打图！");
                return;
            }

            var reply = new List<string> { "/np 给 bleatingsheep，查询更方便！" };
            try
            {
                var ppBeatmap = await s_spider.GetBeatmapPlusAsync(recent.BeatmapId);
                if (ppBeatmap == null)
                {
                    reply.Add("很抱歉，无法查询 Loved 图。也有可能是 PP+ 没有这张图的数据。");
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
