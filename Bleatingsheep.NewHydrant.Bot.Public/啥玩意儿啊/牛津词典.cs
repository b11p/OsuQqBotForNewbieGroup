using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Bleatingsheep.NewHydrant.Attributions;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using Sisters.WudiLib;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊;
#nullable enable
[Component("牛津词典")]
internal partial class 牛津词典 : IMessageCommand
{
    private string _word = null!;
    private readonly ILogger<牛津词典> _logger;

    public 牛津词典(ILogger<牛津词典> logger)
    {
        _logger = logger;
    }

    public async Task ProcessAsync(MessageContext context, HttpApiClient api)
    {
        var url = $"https://www.oxfordlearnersdictionaries.com/definition/american_english/{HttpUtility.UrlPathEncode(_word.Replace(' ', '-'))}";
        await using var page = await Chrome.OpenNewPageAsync();
        await page.SetViewportAsync(new ViewPortOptions
        {
            DeviceScaleFactor = 2,
            Width = 1024,
            Height = 20000,
        });
        var response = await page.GoToAsync(url, WaitUntilNavigation.DOMContentLoaded);
        if ((int)response.Status is >= 400 and < 500)
        {
            // 4xx response
            await api.SendMessageAsync(context.Endpoint, "未找到单词。");
            return;
        }
        if ((int)response.Status is < 200 or >= 500)
        {
            // error
            // 2xx is ok
            // 3xx is redirect
            // 4xx is already handled
            await api.SendMessageAsync(context.Endpoint, $"未知错误：{response.Status}");
            return;
        }

        // GDPR
        var gdprAcceptButton = await page.QuerySelectorAsync("#onetrust-accept-btn-handler");
        if (gdprAcceptButton != null)
        {
            _logger.LogInformation("GDPR clicked");
            await gdprAcceptButton.ClickAsync();
            await Task.Delay(500);
        }

        // remove content advertisements
        var advertisementElements = await page.QuerySelectorAllAsync("div.parallax-container");
        _logger.LogDebug("Removing {count} advertisements", advertisementElements.Length);
        foreach (var adElement in advertisementElements)
        {
            await adElement.EvaluateFunctionAsync("e => e.remove()");
        }

        var mainElement = await page.QuerySelectorAsync("#main-container");
        if (mainElement is null)
        {
            await api.SendMessageAsync(context.Endpoint, "未找到网页元素，可能是网页布局已经更改。");
            return;
        }
        byte[]? mainImage = null;
        for (int i = 0; i < 3; i++)
        {
            try
            {
                mainImage = await mainElement.ScreenshotDataAsync(new ScreenshotOptions
                {
                    Type = ScreenshotType.Png,
                });
            }
            catch (ArgumentException)
            {
                // this may happen if the web page insert advertisements.
                // retry silently.
                _logger.LogError("Getting screenshot error, word: {word}, retry: {i}", _word, i);
            }
        }
        if (mainImage == null)
        {
            // if still fails, fallback to screenshot full page.
            await page.SetViewportAsync(new ViewPortOptions
            {
                DeviceScaleFactor = 2,
                Width = 540,
                Height = 1620,
            });
            mainImage = await page.ScreenshotDataAsync(new ScreenshotOptions
            {
                Type = ScreenshotType.Png,
                FullPage = true,
            });
        }
        await api.SendMessageAsync(context.Endpoint, Message.ByteArrayImage(mainImage));

        try
        {
            var sideElement = await page.QuerySelectorAsync("#rightcolumn");
            if (sideElement is null)
            {
                // silently return;
                return;
            }
            var sideImage = await sideElement.ScreenshotDataAsync(new ScreenshotOptions
            {
                Type = ScreenshotType.Png,
            });
            await api.SendMessageAsync(context.Endpoint, Message.ByteArrayImage(sideImage) + "\r\n若有多个条目，请使用下划线和数字指定要查询的条目，如 model_2");
        }
        catch (Exception)
        {
            // ignore exception.
        }
    }

    public bool ShouldResponse(MessageContext context)
    {
        if (!context.Content.TryGetPlainText(out var text))
        {
            return false;
        }
        var match = GetCommandRegex().Match(text);
        if (!match.Success)
        {
            return false;
        }

        _word = match.Groups[1].Value;
        return true;
    }

    [GeneratedRegex(@"^\s*/ox(?:ford)?\s+(.*?)\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex GetCommandRegex();
}