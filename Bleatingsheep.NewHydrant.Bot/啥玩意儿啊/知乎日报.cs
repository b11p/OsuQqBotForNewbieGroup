using System;
using System.Globalization;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;
using Bleatingsheep.NewHydrant.Attributions;
using HtmlAgilityPack;
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
                bool modified = false;
                var doc = new HtmlDocument();
                var htmlText = item.Summary.Text;
                doc.LoadHtml(htmlText);
                foreach (var imgNode in doc.DocumentNode.SelectNodes("//div/div/div/div/div/p/img")?.Where(n => n.GetClasses().FirstOrDefault() == "content-image") ?? Enumerable.Empty<HtmlNode>())
                {
                    imgNode.SetAttributeValue("width", "100%");
                    modified = true;
                }
                if (modified)
                {
                    htmlText = doc.DocumentNode.InnerHtml;
                }

                using (var page = await Chrome.OpenNewPageAsync())
                {
                    await page.SetContentAsync(htmlText);
                    await page.SetViewportAsync(new ViewPortOptions
                    {
                        DeviceScaleFactor = 3,
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
