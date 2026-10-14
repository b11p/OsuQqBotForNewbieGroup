using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core;
using Bleatingsheep.NewHydrant.Data;
using Bleatingsheep.NewHydrant.Osu;
using PuppeteerSharp;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.Osu
{
    [Component("pptth2")]
    class PpTth2 : Service, IMessageCommand
    {
        //private static readonly object s_launchLock = new object();

        //private static Browser s_browser;

        //private static Func<Browser> GetBrowser { get; set; } = () =>
        //{
        //    lock (s_launchLock)
        //    {
        //        if (s_browser == null)
        //        {
        //            s_browser = Puppeteer.LaunchAsync(new LaunchOptions
        //            {
        //                Headless = true,
        //                ExecutablePath = @"/opt/google/chrome/chrome",
        //                DefaultViewport = new ViewPortOptions
        //                {
        //                    DeviceScaleFactor = 1.2,
        //                    Width = 640,
        //                    Height = 350,
        //                },
        //                Args = new[] { "--no-sandbox" },
        //            }).GetAwaiter().GetResult();
        //        }
        //        if (s_browser != null)
        //            GetBrowser = () => s_browser;
        //        return s_browser;
        //    }
        //};

        private static readonly Regex Regex = new Regex($@"^pptth(?:\s+vs\s+({OsuHelper.UsernamePattern}))?\s*(?:[，,]\s*(.+))?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private string _other;
        private string _mode;

        // 记录成功和失败的时间。
        internal static readonly System.Collections.Concurrent.ConcurrentBag<long> SuccessfulElapsed = new System.Collections.Concurrent.ConcurrentBag<long>();
        internal static readonly System.Collections.Concurrent.ConcurrentBag<long> FailedElapsed = new System.Collections.Concurrent.ConcurrentBag<long>();

        private ILegacyDataProvider DataProvider { get; }
        private OsuMixedApi.OsuApiClient OsuApi { get; }

        public PpTth2(ILegacyDataProvider dataProvider, OsuMixedApi.OsuApiClient osuApi)
        {
            DataProvider = dataProvider;
            OsuApi = osuApi;
        }

        public async Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            //await api.SendMessageAsync(context.Endpoint, $"[DEBUG] 比较：{_other}；模式：{_mode}");

            var id = await DataProvider.EnsureGetBindingIdAsync(context.UserId).ConfigureAwait(false);
            //var browser = GetBrowser();

            byte[] data = null;
            var mode = Bleatingsheep.Osu.Mode.Standard;
            try
            {
                if (!string.IsNullOrEmpty(_mode))
                    mode = Bleatingsheep.Osu.ModeExtensions.Parse(_mode);
            }
            catch
            {
                await api.SendMessageAsync(context.Endpoint, "模式识别失败，fallback 到 Standard。").ConfigureAwait(false);
            }

            var url = $"http://hydrantweb/pptth/mini/{id}?height=350&mode={(int)mode}";
            if (!string.IsNullOrEmpty(_other))
            {
                var (_, user) = await OsuApi.GetUserInfoAsync(_other, mode);
                if (user == null)
                {
                    ExecutingException.Ensure(false, "对比玩家错误");
                }
                url += $"&compared={user.Id}";
            }
            using (var page = await Chrome.OpenNewPageAsync().ConfigureAwait(false))
            {
                await page.SetViewportAsync(new ViewPortOptions
                {
                    DeviceScaleFactor = 1.15,
                    Width = 640,
                    Height = 350,
                }).ConfigureAwait(false);
                await page.GoToAsync(url).ConfigureAwait(false);
                await Task.Delay(0).ConfigureAwait(false);
                data = await page.ScreenshotDataAsync(new ScreenshotOptions
                {
                    FullPage = true,
                    //Type = ScreenshotType.Jpeg,
                    //Quality = 100,
                }).ConfigureAwait(false);
            }

            var stopwatch = Stopwatch.StartNew();
            var sendResponse = await api.SendMessageAsync(context.Endpoint, Message.ByteArrayImage(data)).ConfigureAwait(false);
            var elapsedTime = stopwatch.ElapsedMilliseconds;
            var failed = sendResponse is null;
            if (failed)
            {
                await api.SendMessageAsync(context.Endpoint, "图片发送失败（确定）").ConfigureAwait(false);
            }
            (failed ? FailedElapsed : SuccessfulElapsed).Add(elapsedTime);
        }

        public bool ShouldResponse(MessageContext message)
        {
            if (message is GroupMessage g && g.GroupId == 231094840)
                return false; // ignored in newbie group.
            if (message.Content.TryGetPlainText(out string text))
            {
                var match = Regex.Match(text);
                if (match.Success)
                {
                    _other = match.Groups[1].Value;
                    _mode = match.Groups[2].Value;
                    return true;
                }
                else
                    return false;
            }
            else
            {
                return false;
            }
        }
    }
}
