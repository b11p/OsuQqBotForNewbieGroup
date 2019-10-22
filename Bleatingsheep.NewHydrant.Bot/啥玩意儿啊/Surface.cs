using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using PuppeteerSharp;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊
{
    [Function("surface")]
    internal class Surface : IMessageCommand
    {
        public async Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            using var page = await Chrome.OpenNewPageAsync().ConfigureAwait(false);
            await page.SetViewportAsync(new ViewPortOptions
            {
                DeviceScaleFactor = 1.234,
                Width = 900,
                Height = 1000,
            }).ConfigureAwait(false);
            await page.GoToAsync("https://surface.wiki/").ConfigureAwait(false);
            var listElement = await page.QuerySelectorAsync("body > main > div > div > div.surface-list").ConfigureAwait(false);
            if (listElement == null)
            {
                return;
            }
            var data = await listElement.ScreenshotDataAsync().ConfigureAwait(false);
            await api.SendMessageAsync(context.Endpoint, Message.ByteArrayImage(data)).ConfigureAwait(false);
        }

        public bool ShouldResponse(MessageContext context)
            => context.Content.TryGetPlainText(out var text)
                && string.Equals(text.Trim(), "!surface", StringComparison.OrdinalIgnoreCase);
    }
}
