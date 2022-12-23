﻿using System;
using System.Threading;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace Bleatingsheep.NewHydrant
{
    public static class Chrome
    {
        private static readonly object s_launchLock = new object();

        private static IBrowser s_browser;

        public static Func<IBrowser> GetBrowser { get; private set; } = () =>
        {
            lock (s_launchLock)
            {
                if (s_browser == null)
                {
                    s_browser = Puppeteer.LaunchAsync(new LaunchOptions
                    {
                        Headless = true,
                        //ExecutablePath = @"/opt/google/chrome/google-chrome",
                        //ExecutablePath = @"/usr/bin/chromium-browser",
#if DEBUG
                        ExecutablePath = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
#else
                        ExecutablePath = @"/usr/bin/microsoft-edge",
#endif
                        //ExecutablePath = @"/usr/bin/chromium",
                        DefaultViewport = new ViewPortOptions
                        {
                            DeviceScaleFactor = 1,
                            Width = 360,
                            Height = 640,
                        },
                        Args = new[] { "--no-sandbox", "--lang=zh-CN" },
                    }).GetAwaiter().GetResult();
                }
                if (s_browser != null)
                    GetBrowser = () => s_browser;
                return s_browser;
            }
        };

        public static async Task RefreashBrowserAsync()
        {
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                //ExecutablePath = @"/opt/google/chrome/google-chrome",
                //ExecutablePath = @"/usr/bin/chromium-browser",
                ExecutablePath = @"/usr/bin/microsoft-edge",
                //ExecutablePath = @"/usr/bin/chromium",
                DefaultViewport = new ViewPortOptions
                {
                    DeviceScaleFactor = 1,
                    Width = 360,
                    Height = 640,
                },
                Args = new[] { "--no-sandbox", "--lang=zh-CN" },
            }).ConfigureAwait(false);
            var oldBrowser = Interlocked.Exchange(ref s_browser, browser);
            if (oldBrowser is not null)
            {
                await oldBrowser.DisposeAsync().ConfigureAwait(false);
            }
        }

        public static async Task<IPage[]> GetTabsAsync()
            => await GetBrowser().DefaultContext.PagesAsync().ConfigureAwait(false);

        private static readonly System.Collections.Generic.Dictionary<string, string> s_extraHeaders = new System.Collections.Generic.Dictionary<string, string>()
        {
            ["Accept-Language"] = "zh-CN",
        };

        public static async Task<IPage> OpenNewPageAsync()
        {
            var page = await GetBrowser().NewPageAsync().ConfigureAwait(false);
            await page.SetExtraHttpHeadersAsync(s_extraHeaders).ConfigureAwait(false);
            return page;
        }
    }
}