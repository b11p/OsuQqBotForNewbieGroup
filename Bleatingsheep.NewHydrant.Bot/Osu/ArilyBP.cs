using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.啥玩意儿啊;
using PuppeteerSharp;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.Osu
{
    [Function("arily")]
    internal class ArilyBP : OsuFunction, IMessageCommand
    {
        private static readonly Regex s_regex = new Regex(@"^\s*arily!!\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private string _text;

        public async Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            var uid = await EnsureGetBindingIdAsync(context.UserId).ConfigureAwait(false);
            string url = $"https://p.ri.mk/users/{uid}";
            using var page = await Chrome.OpenNewPageAsync().ConfigureAwait(false);
            await page.SetViewportAsync(new ViewPortOptions
            {
                DeviceScaleFactor = 1,
                Width = 578,
                Height = 80,
            }).ConfigureAwait(false);
            _ = await page.GoToAsync(url).ConfigureAwait(false);
            _ = await page.WaitForSelectorAsync("#finish").ConfigureAwait(false);
            var data = await page.ScreenshotDataAsync(new ScreenshotOptions
            {
                FullPage = true,
                Type = ScreenshotType.Jpeg,
                Quality = 100
            }).ConfigureAwait(false);
            _ = await api.SendMessageAsync(context.Endpoint, Message.ByteArrayImage(data)).ConfigureAwait(false);
        }

        public bool ShouldResponse(MessageContext context)
        {
            return (!(context is GroupMessage g) || g.GroupId == 922534281)
                && context.Content.TryGetPlainText(out _text) && RegexCommand(s_regex, _text);
        }
    }
}
