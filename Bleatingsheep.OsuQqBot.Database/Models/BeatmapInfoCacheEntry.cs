using System;
using System.ComponentModel.DataAnnotations.Schema;
using Bleatingsheep.Osu;
using Bleatingsheep.Osu.ApiClient;
using Microsoft.EntityFrameworkCore;

namespace Bleatingsheep.OsuQqBot.Database.Models;
#nullable enable
[Index(nameof(ExpirationDate))]
public class BeatmapInfoCacheEntry
{
    public int BeatmapId { get; set; }
    public Mode Mode { get; set; }
    public DateTimeOffset CacheDate { get; set; }
    public DateTimeOffset? ExpirationDate { get; set; }
    [Column(TypeName = "jsonb")]
    public required BeatmapInfo? BeatmapInfo { get; set; }
}
#nullable restore