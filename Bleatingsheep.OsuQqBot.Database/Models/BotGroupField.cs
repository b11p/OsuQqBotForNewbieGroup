using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Bleatingsheep.OsuQqBot.Database.Models;
#nullable enable
public class BotGroupField
{
    public long GroupId { get; set; }
    public string FieldName { get; set; } = null!;
    public JsonDocument? Data { get; set; }
    [Timestamp]
    public uint Version { get; set; }
}
#nullable restore