using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using PuppeteerSharp;
using Sisters.WudiLib;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊
{
    [Component("标普500期货")]
    internal class 标普500期货 : IMessageCommand
    {
        public async Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            using var page = await Chrome.OpenNewPageAsync().ConfigureAwait(false);
            await page.SetViewportAsync(new ViewPortOptions
            {
                DeviceScaleFactor = 3.5,
                Width = 800,
                Height = 1000,
            }).ConfigureAwait(false);
            await page.GoToAsync("https://www.investing.com/indices/us-spx-500-futures").ConfigureAwait(false);
            const string selector = "#quotes_summary_current_data";
            var element = await page.QuerySelectorAsync(selector).ConfigureAwait(false);
            var data = await element.ScreenshotDataAsync(new ElementScreenshotOptions
            {
                Type = ScreenshotType.Png,
            }).ConfigureAwait(false);
            bool inLoop = true;
            int retry = 3;
            do
            {
                var mesResponse = await api.SendMessageAsync(context.Endpoint, Message.ByteArrayImage(data)).ConfigureAwait(false);
                inLoop = mesResponse == null;
            } while (inLoop && --retry > 0);
        }

        public bool ShouldResponse(MessageContext context)
            => context.Content.TryGetPlainText(out string text) && text == "标普500";
    }
}
