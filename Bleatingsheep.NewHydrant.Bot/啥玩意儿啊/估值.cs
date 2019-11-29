using System;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core;
using PuppeteerSharp;
using Sisters.WudiLib;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊
{
    [Function("指数估值")]
    class 估值 : Service, IMessageCommand
    {
        public async Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            using (var page = await Chrome.OpenNewPageAsync())
            {
                await page.SetViewportAsync(new ViewPortOptions
                {
                    DeviceScaleFactor = 2,
                    Width = 618,
                    Height = 2000,
                });
                await page.GoToAsync("https://qieman.com/idx-eval");

                //// Get update time.
                //const string updateDateSelector = "#app > div > div.ant-layout-container > div > div > div.ant-col-xs-24.ant-col-lg-15 > div > div > div.sc-jWBwVP.dtKXxN > div > span.qm-header-note > div > p";
                //var updateDateElement = await page.WaitForSelectorAsync(updateDateSelector);
                //if (updateDateElement != null)
                //{
                //    var property = await updateDateElement.GetPropertyAsync("textContent");
                //    if (property != null)
                //    {
                //        var textContent = (await property.JsonValueAsync()).ToString();
                //        await api.SendMessageAsync(context.Endpoint, textContent);
                //    }
                //    else
                //    {
                //        Logger.Debug($"{nameof(property)} is null.");
                //    }
                //}
                //else
                //{
                //    Logger.Debug($"{nameof(updateDateElement)} is null.");
                //}

                //await Task.Delay(TimeSpan.FromSeconds(3));
                //var xp = await page.QuerySelectorAsync("#app > div > div.ant-layout-container > div > div > div.ant-col-xs-24.ant-col-lg-15 > div > div > div.flex-table__67b31");
                //if (xp != null)
                //    await api.SendMessageAsync(context.Endpoint, Message.ByteArrayImage(await xp.ScreenshotDataAsync(new ScreenshotOptions
                //    {

                //    })));


                const string detailSelector = "#app > div > div.ant-layout-container > div > div.ant-row > div > div > div.index__29fc4";

                // delete advertisement
                const string adSelector = "#app > div > div.ant-layout-container > div > div > div > div > div.index__29fc4 > a > img";
                ElementHandle adElement = await page.WaitForSelectorAsync(adSelector).ConfigureAwait(false);
                if (!(adElement is null))
                    await adElement.EvaluateFunctionAsync(@"(element) => { element.parentElement.remove(); }").ConfigureAwait(false);

                ElementHandle detailElement = await page.WaitForSelectorAsync(detailSelector);
                var data = await detailElement
                    .ScreenshotDataAsync(new ScreenshotOptions
                    {
                        //FullPage = true,
                    });

                bool inLoop = true;
                int retry = 3;
                do
                {
                    var mesResponse = await api.SendMessageAsync(context.Endpoint, Message.ByteArrayImage(data)).ConfigureAwait(false);
                    inLoop = mesResponse == null;
                } while (--retry > 0);
            }
        }

        public bool ShouldResponse(MessageContext context)
        {
            return context.Content.TryGetPlainText(out string text)
                && string.Equals(text, "估值", StringComparison.Ordinal);
        }
    }
}
