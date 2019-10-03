using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.啥玩意儿啊;
using Newtonsoft.Json;
using PuppeteerSharp;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.Osu
{
#nullable enable
    [Function("bpme")]
    internal class BPMe : OsuFunction, IMessageCommand
    {
        public async Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            var uid = await EnsureGetBindingIdAsync(context.UserId).ConfigureAwait(false);
            using (var page = await Chrome.OpenNewPageAsync().ConfigureAwait(false))
            {
                await page.SetViewportAsync(new ViewPortOptions
                {
                    DeviceScaleFactor = 1.5,
                    Width = 1440,
                    Height = 900,
                }).ConfigureAwait(false);
                await page.GoToAsync($"https://osu.ppy.sh/users/{uid}/osu").ConfigureAwait(false);

                // wait for load complete
                const string waitSelector = @"body > div.osu-layout__section.osu-layout__section--full.js-content.community_profile > div > div > div > div.osu-layout__section.osu-layout__section--users-extra > div > div > div > div:nth-child(2)";
                await page.WaitForSelectorAsync(waitSelector).ConfigureAwait(false);

                const string bestSelector = "body > div.osu-layout__section.osu-layout__section--full.js-content.community_profile > div > div > div > div.osu-layout__section.osu-layout__section--users-extra > div > div > div > div:nth-child(2) > div > div.play-detail-list";
                //ElementHandle bpsElement = await page.QuerySelectorAsync(bestSelector).ConfigureAwait(false);

                const string buttonSelector = "body > div.osu-layout__section.osu-layout__section--full.js-content.community_profile > div > div > div > div.osu-layout__section.osu-layout__section--users-extra > div > div > div > div:nth-child(2) > div > div.profile-extra-entries__item > button";
                ElementHandle button = await page.QuerySelectorAsync(buttonSelector).ConfigureAwait(false);
                ElementHandle[]? bpList = default;
                int maxTryTimes = 4;
                while (button != null && maxTryTimes-- != 0)
                {
                    //check bp counts.

                    bpList = await page.QuerySelectorAllAsync(bestSelector + " > div").ConfigureAwait(false);

                    await button.ClickAsync().ConfigureAwait(false);

                    // wait for click complete
                    await page.WaitForSelectorAsync(bestSelector + $" > div:nth-child({bpList.Length})",
                        new WaitForSelectorOptions { Timeout = 8000 /*ms*/ }).ConfigureAwait(false);

                    // seems that bp div adding and button availability changing is NOT synchronized
                    // wait for button availability change
                    await page.WaitForTimeoutAsync(500).ConfigureAwait(false);

                    // requery button
                    button = await page.QuerySelectorAsync(buttonSelector).ConfigureAwait(false);
                }

                if (bpList == null)
                {
                    await api.SendMessageAsync(context.Endpoint, "查询失败。").ConfigureAwait(false);
                    return;
                }

                // filter
                /*
                 * bpList = document.querySelectorAll("body > div.osu-layout__section.osu-layout__section--full.js-content.community_profile > div > div > div > div.osu-layout__section.osu-layout__section--users-extra > div > div > div > div:nth-child(2) > div > div.play-detail-list > div");

bpList.forEach((element) => { var dateTime = element.querySelector("div.play-detail__group.play-detail__group--top > div.play-detail__detail > div > span.play-detail__time > time").getAttribute("datetime"); if (new Date() - Date.parse(dateTime) > 86400000) element.remove();  })
                 */

                await page.EvaluateExpressionAsync(@"bpList = document.querySelectorAll(""body > div.osu-layout__section.osu-layout__section--full.js-content.community_profile > div > div > div > div.osu-layout__section.osu-layout__section--users-extra > div > div > div > div:nth-child(2) > div > div.play-detail-list > div"");").ConfigureAwait(false);
                await page.EvaluateExpressionAsync(@"bpList.forEach((element) => { var dateTime = element.querySelector(""div.play-detail__group.play-detail__group--top > div.play-detail__detail > div > span.play-detail__time > time"").getAttribute(""datetime""); if (new Date() - Date.parse(dateTime) > 86400000) element.remove();  })").ConfigureAwait(false);
                // check
                bpList = await page.QuerySelectorAllAsync(bestSelector + " > div").ConfigureAwait(false);
                if (bpList.Length == 0)
                {
                    await api.SendMessageAsync(context.Endpoint, "最近 24 小时没有更新 bp。").ConfigureAwait(false);
                    return;
                }

                //screenshot
                //delete pinned elements
                await (await page.QuerySelectorAsync("body > div.js-pinned-header.hidden-xs.no-print.nav2-header > div.nav2-header__body").ConfigureAwait(false)).EvaluateFunctionAsync(@"(element) => element.remove()").ConfigureAwait(false);
                await (await page.QuerySelectorAsync("body > div.osu-layout__section.osu-layout__section--full.js-content.community_profile > div > div > div > div.hidden-xs.page-extra-tabs.page-extra-tabs--profile-page.js-switchable-mode-page--scrollspy-offset").ConfigureAwait(false)).EvaluateFunctionAsync(@"(element) => element.remove()").ConfigureAwait(false);

                ElementHandle bpsElement = await page.QuerySelectorAsync(bestSelector).ConfigureAwait(false);
                var data = await bpsElement.ScreenshotDataAsync(new ScreenshotOptions()).ConfigureAwait(false);

                await api.SendMessageAsync(context.Endpoint, Message.ByteArrayImage(data)).ConfigureAwait(false);
            }
        }

        public bool ShouldResponse(MessageContext context)
            => context.Content.TryGetPlainText(out var text)
            && string.Equals(text.Trim(), "print bp recent", StringComparison.OrdinalIgnoreCase);
    }
#nullable restore
}
