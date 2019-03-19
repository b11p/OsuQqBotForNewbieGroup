using System;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊
{
    public static class Chrome
    {
        private static readonly object s_launchLock = new object();

        private static Browser s_browser;

        public static Func<Browser> GetBrowser { get; private set; } = () =>
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
                            DeviceScaleFactor = 1,
                            Width = 360,
                            Height = 640,
                        },
                        Args = new[] { "--no-sandbox" },
                    }).GetAwaiter().GetResult();
                }
                if (s_browser != null)
                    GetBrowser = () => s_browser;
                return s_browser;
            }
        };

        public static async Task<Page> OpenNewPageAsync()
            => await GetBrowser().NewPageAsync();
    }
}
