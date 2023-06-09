using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Sisters.WudiLib;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊.PushNotifications;
#nullable enable
internal class AddIyuu : IMessageCommand
{
    public Task ProcessAsync(MessageContext context, HttpApiClient api)
    {
        string token = context.Content.Text[5..].Trim();
        throw new NotImplementedException();

    }

    public bool ShouldResponse(MessageContext context)
    {
        return context.Content.TryGetPlainText(out string text) && text.StartsWith("iyuu ", StringComparison.OrdinalIgnoreCase);
        throw new NotImplementedException();
    }
}
#nullable restore