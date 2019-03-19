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

namespace Bleatingsheep.NewHydrant.啥玩意儿啊
{
    [Function("zhihu_daily")]
    public class 知乎日报 : IMessageCommand
    {
        public async Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            string url = "https://rss.bleatingsheep.org/zhihu/daily";
            var xmlReader = XmlReader.Create(url);
            var feed = SyndicationFeed.Load(xmlReader);
            xmlReader.Close();
            var item = feed.Items.LastOrDefault();
            if (item != default)
            {
                using (var page = await Chrome.OpenNewPageAsync())
                {
                    await page.SetContentAsync(item.Summary.Text);
                    await page.SetViewportAsync(new ViewPortOptions
                    {
                        DeviceScaleFactor = 1,
                        Width = 360,
                        Height = 640,
                    });
                    var data = await page.ScreenshotDataAsync(new ScreenshotOptions
                    {
                        FullPage = true,
                    });
                    await Task.Delay(100);
                    await api.SendMessageAsync(context.Endpoint, Message.ByteArrayImage(data));
                }
            }
        }

        public bool ShouldResponse(MessageContext context)
            => context.Content.TryGetPlainText(out string text) && text == "知乎日报";
    }
}
