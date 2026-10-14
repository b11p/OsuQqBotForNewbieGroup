using System;
using System.Threading;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace Bleatingsheep.NewHydrant;
#nullable enable
public static class Chrome
{
    private static readonly SemaphoreSlim s_semaphoreSlim = new(1, 1);

    private static IBrowser? s_browser;

    public static string? ChromePath { get; set; }

    private static Task<IBrowser> LaunchBrowser()
    {
        if (ChromePath == null)
        {
            throw new InvalidOperationException("未设置 Chrome 浏览器的路径。");
        }

        return Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            ExecutablePath = ChromePath,
            DefaultViewport = new ViewPortOptions
            {
                DeviceScaleFactor = 1,
                Width = 360,
                Height = 640,
            },
            Args = new[] { "--no-sandbox", "--lang=zh-CN", "--font-render-hinting=none" },
        });
    }

    private static Func<ValueTask<IBrowser>> GetBrowser { get; set; } = async () =>
    {
        await s_semaphoreSlim.WaitAsync();
        try
        {
            s_browser ??= await LaunchBrowser();
            GetBrowser = () => ValueTask.FromResult(s_browser);
            return s_browser;
        }
        finally
        {
            s_semaphoreSlim.Release();
        }
    };

    public static async Task RefreashBrowserAsync()
    {
        IBrowser browser = await LaunchBrowser();
        var oldBrowser = Interlocked.Exchange(ref s_browser, browser);
        if (oldBrowser is not null)
        {
            await oldBrowser.DisposeAsync().ConfigureAwait(false);
        }
    }

    public static async Task<IPage[]> GetTabsAsync()
        => await (await GetBrowser()).DefaultContext.PagesAsync().ConfigureAwait(false);

    private static readonly System.Collections.Generic.Dictionary<string, string> s_extraHeaders = new()
    {
        ["Accept-Language"] = "zh-CN",
    };

    public static async Task<IPage> OpenNewPageAsync()
    {
        var page = await (await GetBrowser()).NewPageAsync().ConfigureAwait(false);
        await page.SetExtraHttpHeadersAsync(s_extraHeaders).ConfigureAwait(false);
        return page;
    }
}
