using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core;
using Sisters.WudiLib;
using Message = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊.Moebooru
{
    [Function("konachan")]
    class Konachan : IMessageCommand
    {
        private static readonly ISet<string> DissTags = new HashSet<string>
        {
            "bikini",
            "panties",
            "nude",
            "bikini_top",
            "breast_hold",
            "breasts",
            "cleavage",
            "all_male",
            "see_through",
            "ass",
            "underboob",
            "swimsuit",
            "barefoot",
            "pantyhose",
            "garter_belt",
            "bodysuit",
        };

        public async Task ProcessAsync(Message message, HttpApiClient api, ExecutingInfo executingInfo)
        {
            var k = new Api("https://konachan.net");
            var recent = await k.PopularRecentAsync();
            if (recent == null)
                return;

            recent = recent.Where(p => !p.tags.Split().Intersect(DissTags).Any()).Take(1);
            foreach (var post in recent)
            {
                await api.SendMessageAsync(message.Endpoint, SendingMessage.NetImage(post.JpegUrl));
            }
        }

        public bool ShouldResponse(Message message)
        {
            return message.Content.Text.StartsWith("健康konachan", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
