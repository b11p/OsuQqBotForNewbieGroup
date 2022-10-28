using System;
using System.Collections.Generic;
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
        var messageList = new List<string>();
        messageList.Add($"{loadArray[0]} {loadArray[1]} {loadArray[2]}");
        // messageList.Add(context.MessageId.ToString(CultureInfo.InvariantCulture));
        messageList.Add(context.Time.ToOffset(TimeZoneInfo.FindSystemTimeZoneById("America/Toronto").GetUtcOffset(DateTimeOffset.UtcNow)).ToString("H:mm:ss"));

        var pressureio = File.ReadAllText("/proc/pressure/io");
        var pressureioArray = pressureio.Split();
        messageList.Add($"IO Pressure avg300: some {pressureioArray[3][7..]}, full {pressureioArray[8][7..]}");

        var pressureMem = File.ReadAllText("/proc/pressure/memory");
        var pressureMemArray = pressureMem.Split();
        if (double.Parse(pressureMemArray[3][7..]) > 0)
        {
            messageList.Add($"Memory Pressure avg300: some {pressureMemArray[3][7..]}, full {pressureMemArray[8][7..]}");
        }

        await api.SendMessageAsync(context.Endpoint, string.Join("\r\n", messageList)).ConfigureAwait(false);
    }

    public bool ShouldResponse(MessageContext context)
        => context.UserId == 962549599 && context.Content.TryGetPlainText(out var text) && "loadavg".Equals(text, StringComparison.OrdinalIgnoreCase);
}
