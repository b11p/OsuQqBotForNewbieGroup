using System;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using PuppeteerSharp;
using Sisters.WudiLib;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊;

[Component("us_covid_19")]
internal class 美国新冠数据 : IMessageCommand
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
        await page.GoToAsync("https://www.nytimes.com/interactive/2021/us/covid-cases.html").ConfigureAwait(false);
        await page.WaitForSelectorAsync("#us-covid-cases > div > div > main > div.g-columns-outer.svelte-hfvvmm > div:nth-child(2) > section:nth-child(1)").ConfigureAwait(false);
        var deleteElement = await page.QuerySelectorAsync("#standalone-footer > div > div").ConfigureAwait(false);
        await deleteElement.EvaluateFunctionAsync("b => b.remove()").ConfigureAwait(false);
        var element = await page.QuerySelectorAsync("#__covidtracker__ > main > div.g-columns-outer.svelte-hfvvmm > div:nth-child(1) > section:nth-child(1)").ConfigureAwait(false);
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
        => context.Content.TryGetPlainText(out var text) && "美国完了吗".Equals(text, StringComparison.Ordinal);
}
