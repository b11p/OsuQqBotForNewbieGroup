using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Sisters.WudiLib;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊
{
    [Function("ciya")]
    public class Ciya : IMessageCommand
    {
        public Task ProcessAsync(Sisters.WudiLib.Posts.Message context, HttpApiClient api)
            => api.SendMessageAsync(context.Endpoint, context.Content.Forward());

        public bool ShouldResponse(Sisters.WudiLib.Posts.Message context)
        {
            IReadOnlyList<Section> sections = context.Content.Sections;
            return sections.Count == 3 && sections.All(s => s.Type == "face" && s.Data.GetValueOrDefault("id") == "13");
        }
    }
}
