using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.啥玩意儿啊;
using Sisters.WudiLib;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.Tests
{
    [Component("chrome_relaunch")]
    class ChromeRelaunch : IMessageCommand
    {
        public async Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            await Chrome.RefreashBrowserAsync().ConfigureAwait(false);
            await api.SendMessageAsync(context.Endpoint, "重启完毕。").ConfigureAwait(false);
        }

        public bool ShouldResponse(MessageContext context)
        {
            return context.UserId == 962549599
                && context.Content.TryGetPlainText(out string text)
                && text == "重启浏览器";
        }
    }
}
