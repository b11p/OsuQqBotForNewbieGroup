using System;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using PuppeteerSharp;
using Sisters.WudiLib;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊;

[Component("canada_covid_19")]
internal class 加拿大新冠数据 : IMessageCommand
{
    public async Task ProcessAsync(MessageContext context, HttpApiClient api)
    {
        using var page = await Chrome.OpenNewPageAsync().ConfigureAwait(false);
        await page.SetViewportAsync(new ViewPortOptions
        {
            DeviceScaleFactor = 3,
            Width = 360,
            Height = 8000,
        }).ConfigureAwait(false);
        await page.GoToAsync("https://en.wikipedia.org/wiki/Template:COVID-19_pandemic_data/Canada_medical_cases_by_province").ConfigureAwait(false);
        var element = await page.QuerySelectorAsync("#mw-content-text > div.mw-parser-output > table").ConfigureAwait(false);
        await page.SetViewportAsync(new ViewPortOptions
        {
            DeviceScaleFactor = 2,
            Width = 1024,
            Height = 4000,
        }).ConfigureAwait(false);
        var data2 = await element.ScreenshotDataAsync(new ElementScreenshotOptions
        {
            Type = ScreenshotType.Jpeg,
            Quality = 100,
        }).ConfigureAwait(false);
        await api.SendMessageAsync(context.Endpoint, Message.ByteArrayImage(data2)).ConfigureAwait(false);
    }

    public bool ShouldResponse(MessageContext context)
        => context.Content.TryGetPlainText(out var text) && "加拿大完了吗".Equals(text, StringComparison.Ordinal);
}
