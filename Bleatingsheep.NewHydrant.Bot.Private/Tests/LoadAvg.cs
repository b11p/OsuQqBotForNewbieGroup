using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Sisters.WudiLib;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.Tests;

[Component("loadavg")]
class LoadAvg : IMessageCommand
{
    public async Task ProcessAsync(MessageContext context, HttpApiClient api)
    {
        var load = File.ReadAllText("/proc/loadavg");
        var loadArray = load.Split();
        var messageArray = new string[3];
        messageArray[0] = $"{loadArray[0]} {loadArray[1]} {loadArray[2]}";
        messageArray[1] = context.MessageId.ToString(CultureInfo.InvariantCulture);
        messageArray[2] = context.Time.ToOffset(TimeSpan.FromHours(9)).ToString("H:mm:ss");
        await api.SendMessageAsync(context.Endpoint, string.Join("\r\n", messageArray)).ConfigureAwait(false);
    }

    public bool ShouldResponse(MessageContext context)
        => context.UserId == 962549599 && context.Content.TryGetPlainText(out var text) && "loadavg".Equals(text, StringComparison.OrdinalIgnoreCase);
}
