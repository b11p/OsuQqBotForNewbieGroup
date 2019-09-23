using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.啥玩意儿啊;
using PuppeteerSharp;
using Sisters.WudiLib;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.Osu
{
    //[Function("print_screen")]
    internal class 个人信息网页截图 : OsuFunction, IMessageCommand
    {
        private static readonly Regex s_regex = new Regex($"^users?/(?<name>{OsuHelper.UsernamePattern})/(?<mode>.*)$");

        [Parameter("mode")]
        public string Mode { get; set; }

        [Parameter("name")]
        public string Name { get; set; }

        public async Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            Message message;
            using (var page = await Chrome.OpenNewPageAsync())
            {
                await page.SetViewportAsync(new ViewPortOptions
                {
                    DeviceScaleFactor = 1,
                    Width = 550,
                    Height = 725,
                });
                await page.GoToAsync($"https://osu.ppy.sh/users/{Name}/{Mode}");
                const string chartSelector = "body > div.osu-layout__section.osu-layout__section--full.js-content.community_profile > div > div > div > div.js-switchable-mode-page--scrollspy.js-switchable-mode-page--page";
                ElementHandle detailElement = await page.WaitForSelectorAsync(chartSelector);
                var data = await detailElement
                    .ScreenshotDataAsync(new ScreenshotOptions
                    {
                    });
                message = Message.ByteArrayImage(data);
            }
            await api.SendMessageAsync(context.Endpoint, message);
        }

        public bool ShouldResponse(MessageContext context)
        {
            return RegexCommand(s_regex, context.Content);
        }
    }
}
