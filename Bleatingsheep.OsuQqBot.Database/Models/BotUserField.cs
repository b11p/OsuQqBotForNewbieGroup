using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Bleatingsheep.OsuQqBot.Database.Models;
#nullable enable
public class BotUserField
{
    public long UserId { get; set; }
    public string FieldName { get; set; } = null!;
    public JsonDocument? Data { get; set; }
    [Timestamp]
    public byte[] Version { get; set; } = null!;
}
#nullable restore