using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.啥玩意儿啊;
using PuppeteerSharp;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.Osu
{
    [Component("arily")]
    internal class ArilyBP : OsuFunction, IMessageCommand
    {
        private static readonly Regex s_regex = new Regex(@"^\s*arily[1!！]{2}\s*(?<name>" + OsuHelper.UsernamePattern + @")?\s*(?:[，,]\s*(?<date>.*?))?\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private string _text;

        [Parameter("name")]
        public string Name { get; set; }

        [Parameter("date")]
        public string DateTimeText { get; set; }

        public async Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            int uid;
            if (!string.IsNullOrEmpty(DateTimeText))
            {
                if (DateTime.TryParse(DateTimeText, out var date))
                {
                    DateTimeText = date.ToString("yyyy-M-d");
                }
                else
                {
                    //await api.SendMessageAsync(context.Endpoint, "日期格式错误").ConfigureAwait(false);
                    //return;
                }
            }

            if (string.IsNullOrEmpty(Name))
            {
                uid = await EnsureGetBindingIdAsync(context.UserId).ConfigureAwait(false);
            }
            else
            {
                var user = await EnsureGetUserInfo(Name, Bleatingsheep.Osu.Mode.Standard).ConfigureAwait(false);
                uid = user.Id;
            }
            string url = $"https://p.ri.mk/users/{uid}/{DateTimeText}";
            using var page = await Chrome.OpenNewPageAsync().ConfigureAwait(false);
            await page.SetViewportAsync(new ViewPortOptions
            {
                DeviceScaleFactor = Math.Sqrt(2),
                Width = 700,
                Height = 80,
            }).ConfigureAwait(false);
            _ = await page.GoToAsync(url).ConfigureAwait(false);
            _ = await page.WaitForSelectorAsync("#finish").ConfigureAwait(false);
            //await Task.Delay(0).ConfigureAwait(false);
            var data = await page.ScreenshotDataAsync(new ScreenshotOptions
            {
                FullPage = true,
                Type = ScreenshotType.Jpeg,
                Quality = 100,
            }).ConfigureAwait(false);
            _ = await api.SendMessageAsync(context.Endpoint, Message.ByteArrayImage(data)).ConfigureAwait(false);
        }

        public bool ShouldResponse(MessageContext context)
        {
            return
                //(!(context is GroupMessage g) || g.GroupId == 922534281) &&
                context.Content.TryGetPlainText(out _text) && RegexCommand(s_regex, _text);
        }
    }
}
