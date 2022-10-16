using System;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using PuppeteerSharp;
using Sisters.WudiLib;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊
{
    [Component("japan_covid_19")]
    internal class 日本新冠数据 : IMessageCommand
    {
        public async Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            using var page = await Chrome.OpenNewPageAsync().ConfigureAwait(false);
            await page.SetViewportAsync(new ViewPortOptions
            {
                DeviceScaleFactor = 3,
                Width = 360,
                Height = 8000,
            }).ConfigureAwait(false);
            await page.GoToAsync("https://toyokeizai.net/sp/visual/tko/covid19/index.html").ConfigureAwait(false);
            await page.WaitForSelectorAsync("#main-block > div:nth-child(2) > div:nth-child(1) > div > div.charts-wrapper > div.main-chart-wrapper > div > canvas").ConfigureAwait(false);
            var element = await page.QuerySelectorAsync("#main-block > div:nth-child(2)").ConfigureAwait(false);
            await page.SetViewportAsync(new ViewPortOptions
            {
                DeviceScaleFactor = 2,
                Width = 1024,
                Height = 4000,
            }).ConfigureAwait(false);
            var data2 = await element.ScreenshotDataAsync(new ScreenshotOptions
            {
                Type = ScreenshotType.Jpeg,
                Quality = 100,
            }).ConfigureAwait(false);
            await api.SendMessageAsync(context.Endpoint, Message.ByteArrayImage(data2)).ConfigureAwait(false);
        }

        public bool ShouldResponse(MessageContext context)
            => context.Content.TryGetPlainText(out var text) && "日本完了吗".Equals(text, StringComparison.Ordinal);
    }
}
