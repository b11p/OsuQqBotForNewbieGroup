using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core;
using Bleatingsheep.NewHydrant.Data;
using Bleatingsheep.Osu;
using Bleatingsheep.Osu.ApiClient;
using PuppeteerSharp;
using Sisters.WudiLib;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.Osu
{
    [Component("arily")]
    internal class ArilyInfo : Service, IMessageCommand
    {
        private static readonly Regex s_regex = new Regex(@"^\s*arily[1!！]{2}\s*(?<name>" + OsuHelper.UsernamePattern + @")?\s*[,，]?\s*(?<mode>\S*)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private static readonly IReadOnlyDictionary<Mode, string> s_modes = new Dictionary<Mode, string>
        {// mode是['osu','taiko','fruits','mania']
            { Mode.Standard, "osu" },
            { Mode.Taiko, "taiko" },
            { Mode.Catch, "fruits" },
            { Mode.Mania, "mania" },
        };
        private readonly IOsuApiClient _osuApiClient;

        public ArilyInfo(ILegacyDataProvider dataProvider, IOsuApiClient osuApiClient)
        {
            DataProvider = dataProvider;
            _osuApiClient = osuApiClient;
        }

        private string _text;

        [Parameter("name")]
        public string Name { get; set; }

        [Parameter("mode")]
        public string ModeString { get; set; }
        private ILegacyDataProvider DataProvider { get; }

        public async Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            long uid;
            Mode mode;
            try
            {
                mode = ModeExtensions.Parse(ModeString);
            }
            catch
            {
                mode = default;
            }

            if (string.IsNullOrEmpty(Name))
            {
                uid = await DataProvider.EnsureGetBindingIdAsync(context.UserId).ConfigureAwait(false);
            }
            else
            {
                var userInfo = await _osuApiClient.GetUser(Name, Mode.Standard);
                if (userInfo == null)
                {
                    await api.SendMessageAsync(context.Endpoint, "没有找到此用户名对应的用户。");
                    return;
                }
                uid = userInfo.Id;
            }
            string url = $"https://info.osustuff.ri.mk/cn/users/{uid}/{s_modes.GetValueOrDefault(mode)}";
            await using var page = await Chrome.OpenNewPageAsync().ConfigureAwait(false);
            await page.SetViewportAsync(new ViewPortOptions
            {
                DeviceScaleFactor = Math.Sqrt(0.5),
                Width = 1058,
                Height = 80,
            }).ConfigureAwait(false);
            _ = await page.GoToAsync(url, WaitUntilNavigation.Networkidle0).ConfigureAwait(false);
            await Task.Delay(500).ConfigureAwait(false);
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
