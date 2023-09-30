using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Microsoft.Extensions.Logging;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊;
#nullable enable
[Component(nameof(获取图片链接))]
internal partial class 获取图片链接 : IMessageCommand
{
    private readonly ILogger<获取图片链接> _logger;

    public 获取图片链接(ILogger<获取图片链接> logger)
    {
        _logger = logger;
    }

    private string _command = default!;

    public async Task ProcessAsync(MessageContext context, HttpApiClient api)
    {
        var url = await CitedImageUrlUtility.GetCitedImageUrlAsync(context, api, _logger);
        await api.SendMessageAsync(context.Endpoint, url);
    }

    public bool ShouldResponse(MessageContext context)
    {
        var regex = GetCommandRegex();
        var command = context switch
        {
            GroupMessage g => g.Content.MergeContinuousTextSections().Sections.Where(s => s.Type != Section.TextType || !string.IsNullOrWhiteSpace(s.Data[Section.TextParamName])).ToList() switch
            {
                [{ Type: "reply" }, { Type: "at" }, { Type: "at" }, { Type: "text" } s, ..] => s.Data["text"],
                [{ Type: "reply" }, { Type: "at" }, { Type: "text" } s, ..] => s.Data["text"],
                [{ Type: "reply" }, { Type: "text" } s, ..] => s.Data["text"],
                _ => default,
            },
            _ => default,
        };
        if (string.IsNullOrWhiteSpace(command))
        {
            if (context.Content.Raw.Contains("/url", StringComparison.InvariantCultureIgnoreCase))
            {
                // only for debug.
                return true;
            }
            return false;
        }
        if (regex.IsMatch(command))
        {
            _command = command;
            return true;
        }
        return false;
    }

    [GeneratedRegex(@"^\s*/url(?:\s|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex GetCommandRegex();
}
