using System;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;
using Bleatingsheep.NewHydrant.Attributions;
using PuppeteerSharp;
using Sisters.WudiLib;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.Tests
{
    [Component("rend")]
    internal class 渲染任意页面 : IMessageCommand
    {
        private string _text;
        public async Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            var rendArgs = _text.Substring("rend ".Length);
            var rendArgsArray = rendArgs.Split();
            Uri uri = default;
            string html = default;
            if (rendArgsArray[0] != "rss")
            {
                uri = new Uri(rendArgs);
            }
            else
            {
                // RSS
                var rssUrl = rendArgsArray[1];
                var pageUrl = rendArgsArray[2];
                var xmlReader = XmlReader.Create(rssUrl);
                var feed = SyndicationFeed.Load(xmlReader);
                xmlReader.Close();
                var wantedItem = feed.Items.FirstOrDefault(i => string.Equals(i.Links?.FirstOrDefault()?.Uri?.ToString(), pageUrl, StringComparison.OrdinalIgnoreCase));
                if (wantedItem != null)
                    html = wantedItem.Summary.Text;
                else
                    // 没有合理命中的项目。
                    uri = new Uri(pageUrl);
            }

            byte[] data;

            //using (browser)
            using (var page = await Chrome.OpenNewPageAsync())
            {
                await page.SetViewportAsync(new ViewPortOptions
                {
                    DeviceScaleFactor = 1.5,
                    Width = 360,
                    IsMobile = true,
                    Height = 30640,
                    HasTouch = true,
                });
                if (uri != null)
                    await page.GoToAsync(uri.AbsoluteUri).ConfigureAwait(false);
                else
                    await page.SetContentAsync(html);

                // Wait for 10 seconds to load dynamic contents.
                //await Task.Delay(10000).ConfigureAwait(false);

                await page.SetViewportAsync(new ViewPortOptions
                {
                    DeviceScaleFactor = 1.5,
                    Width = 360,
                    IsMobile = true,
                    Height = 640,
                    HasTouch = true,
                }).ConfigureAwait(false);

                data = await page.ScreenshotDataAsync(new ScreenshotOptions
                {
                    FullPage = true,
                    Type = ScreenshotType.Jpeg,
                    Quality = 100,
                }).ConfigureAwait(false);

                // Save file.
                //const string output_dir = "/root/outputs";
                //var file_name = DateTime.Now.ToString("yyyy-MM-dd HH：mm：ss") + ".jpg";
                //var path = Path.Combine(output_dir, file_name);
                //await File.WriteAllBytesAsync(path, data).ConfigureAwait(false);
                //var pictureUri = new Uri($"https://res.bleatingsheep.org/{file_name}");
                //await api.SendMessageAsync(context.Endpoint, pictureUri.AbsoluteUri).ConfigureAwait(false);
            }

            await api.SendMessageAsync(context.Endpoint, Message.ByteArrayImage(data));
        }

        public bool ShouldResponse(MessageContext context)
            => context.UserId == 962549599
            && context.Content.TryGetPlainText(out _text)
            && _text.StartsWith("rend ", StringComparison.InvariantCultureIgnoreCase);
    }
}
