using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Bleatingsheep.OsuQqBot.Database.Models;
#nullable enable
public sealed class BotUserField : IDisposable
{
    public long UserId { get; set; }
    public string FieldName { get; set; } = null!;
    public JsonDocument? Data { get; set; }
    [Timestamp]
    public uint Version { get; set; }

    public void Dispose()
    {
        Data?.Dispose();
    }
}
#nullable restore