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
        private static readonly object s_launchLock = new object();

        private static Browser s_browser;

        private static Func<Browser> GetBrowser { get; set; } = () =>
        {
            lock (s_launchLock)
            {
                if (s_browser == null)
                {
                    s_browser = Puppeteer.LaunchAsync(new LaunchOptions
                    {
                        Headless = true,
                        ExecutablePath = @"/opt/google/chrome/chrome",
                        DefaultViewport = new ViewPortOptions
                        {
                            DeviceScaleFactor = 3,
                            Width = 360,
                            Height = 350,
                        },
                        Args = new[] { "--no-sandbox" },
                    }).GetAwaiter().GetResult();
                }
                if (s_browser != null)
                    GetBrowser = () => s_browser;
                return s_browser;
            }
        };

        public async Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            string url = "https://rss.bleatingsheep.org/zhihu/daily";
            var xmlReader = XmlReader.Create(url);
            var feed = SyndicationFeed.Load(xmlReader);
            xmlReader.Close();
            var item = feed.Items.LastOrDefault();
            if (item != default)
            {
                using (var page = await GetBrowser().NewPageAsync())
                {
                    await page.SetContentAsync(item.Summary.Text);
                    await page.SetViewportAsync(new ViewPortOptions
                    {
                        DeviceScaleFactor = 3,
                        Width = 360,
                        Height = 350,
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
