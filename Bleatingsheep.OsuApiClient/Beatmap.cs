using Newtonsoft.Json;
using System;

namespace Bleatingsheep.OsuMixedApi
{
    public class Beatmap
    {
        [JsonProperty("beatmapset_id")]
        public int Sid { get; set; }
        [JsonProperty("beatmap_id")]
        public int Bid { get; set; }
        [JsonProperty("approved")]
        public Approved Approved { get; set; }
        [JsonProperty("total_length")]
        public int TotalLength { get; set; }
        [JsonProperty("hit_length")]
        public int HitLength { get; set; }
        [JsonProperty("version")]
        public string DifficultyName { get; set; }
        [JsonProperty("file_md5")]
        public string FileMD5 { get; set; }
        [JsonProperty("diff_size")]
        public double CS { get; set; }
        [JsonProperty("diff_overall")]
        public double OD { get; set; }
        [JsonProperty("diff_approach")]
        public double AR { get; set; }
        [JsonProperty("diff_drain")]
        public double HP { get; set; }
        [JsonProperty("mode")]
        public Mode Mode { get; set; }

        [JsonProperty("approved_date")]
        private DateTime? ApprovedDate { get; set; }
        [JsonIgnore]
        public DateTimeOffset? ApprovedDateOffset
        {
            get
            {
                var appDate = ApprovedDate;
                if (appDate.HasValue)
                    return new DateTimeOffset(appDate.Value, OsuApiClient.TimeZone);
                return null;
            }
            private set
            {
                if (value == null)
                {
                    ApprovedDate = null;
                    return;
                }
                ApprovedDate = value.Value.ToOffset(OsuApiClient.TimeZone).DateTime;
            }
        }

        [JsonProperty("last_update")]
        private DateTime LastUpdate { get; set; }
        [JsonIgnore]
        public DateTimeOffset LastUpdateOffset
        {
            get => new DateTimeOffset(LastUpdate, OsuApiClient.TimeZone);
            private set => LastUpdate = value.ToOffset(OsuApiClient.TimeZone).DateTime;
        }

        [JsonProperty("artist")]
        public string Artist { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("creator")]
        public string Creator { get; set; }
        [JsonProperty("bpm")]
        public double Bpm { get; set; }
        [JsonProperty("source")]
        public string Source { get; set; }
        [JsonProperty("tags")]
        public string Tags { get; set; }
        [JsonProperty("genre_id")]
        public Genre Genre { get; set; }
        [JsonProperty("language_id")]
        public Language Language { get; set; }
        [JsonProperty("favourite_count")]
        public int FavoriteCount { get; set; }
        [JsonProperty("playcount")]
        public long PlayCount { get; set; }
        [JsonProperty("passcount")]
        public long PassCount { get; set; }
        [JsonProperty("max_combo")]
        public int? MaxCombo { get; set; }
        [JsonProperty("difficultyrating")]
        public double Stars { get; set; }
    }

    public enum Approved
    {
        Loved = 4,
        Qualified = 3,
        Approved = 2,
        Ranked = 1,
        Pending = 0,
        Wip = -1,
        Graveryard = -2,
    }

    public enum Genre
    {
        Any = 0,
        Unspecified = 1,
        VideoGame = 2,
        Anime = 3,
        Rock = 4,
        Pop = 5,
        Other = 6,
        Novelty = 7,
        HipHop = 9,
        Electronic = 10,
    }

    public enum Language
    {
        Any = 0,
        Other = 1,
        English = 2,
        Japanese = 3,
        Chinese = 4,
        Instrumental = 5,
        Korean = 6,
        French = 7,
        German = 8,
        Swedish = 9,
        Spanish = 10,
        Italian = 11,
    }
}
