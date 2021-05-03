using System;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.啥玩意儿啊;
using Sisters.WudiLib;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.Tests
{
    [Component("chrome_tab_count_report")]
    public class ChromeTabCountReport : IMessageCommand
    {
        public async Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            var tabs = await Chrome.GetTabsAsync().ConfigureAwait(false);
            await api.SendMessageAsync(context.Endpoint, $"已打开了 {tabs.Length} 个标签页。").ConfigureAwait(false);
        }

        public bool ShouldResponse(MessageContext context)
            => context.UserId == 962549599
            && context.Content.TryGetPlainText(out string text)
            && string.Equals("tabs", text, StringComparison.OrdinalIgnoreCase);
    }
}
