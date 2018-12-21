using Newtonsoft.Json;

namespace Bleatingsheep.OsuMixedApi
{
#pragma warning disable CS0649
    [JsonObject(MemberSerialization.OptIn)]
    public class UserInfo
    {
        [JsonProperty("user_id")]
        public int Id { get; private set; }
        [JsonProperty("username")]
        public string Name { get; private set; }
        public Mode Mode { get; internal set; }
        [JsonProperty("count300")]
        private int? count300;
        public int Count300 => DefaultIfNull(count300);
        [JsonProperty("count100")]
        private int? count100;
        public int Count100 => DefaultIfNull(count100);
        [JsonProperty("count50")]
        private int? count50;
        public int Count50 => DefaultIfNull(count50);
        public int TotalHits => Count300 + Count100 + Count50;
        [JsonProperty("playcount")]
        private int? playCount;
        public int PlayCount => DefaultIfNull(playCount);
        [JsonProperty("ranked_score")]
        private long? rankedScore;
        public long RankedScore => DefaultIfNull(rankedScore);
        [JsonProperty("total_score")]
        private long? totalScore;
        public long TotalScore => DefaultIfNull(totalScore);
        [JsonProperty("pp_rank")]
        private int? rank;
        public int Rank => DefaultIfNull(rank);
        [JsonProperty("level")]
        private double? level;
        public double Level => DefaultIfNull(level);
        [JsonProperty("pp_raw")]
        private double? performance;
        public double Performance => DefaultIfNull(performance);
        [JsonProperty("accuracy")]
        private double? accuracy;
        public double Accuracy => DefaultIfNull(accuracy) / 100;
        [JsonProperty("count_rank_ss")]
        private int? countSs;
        public int CountSs => DefaultIfNull(countSs);
        [JsonProperty("count_rank_ssh")]
        private int? countSsh;
        public int CountSsh => DefaultIfNull(countSsh);
        [JsonProperty("count_rank_s")]
        private int? countS;
        public int CountS => DefaultIfNull(countS);
        [JsonProperty("count_rank_sh")]
        private int? countSh;
        public int CountSh => DefaultIfNull(countSh);
        [JsonProperty("count_rank_a")]
        private int? countA;
        public int CountA => DefaultIfNull(countA);
        [JsonProperty("country")]
        public string CountryCode { get; private set; }
        public string Country => Iso3166.CountryOf(CountryCode);
        [JsonProperty("pp_country_rank")]
        public int? CountryRank { get; private set; }
        [JsonProperty("events")]
        public Event[] Events { get; private set; }

        private UserInfo() { }

        private static T DefaultIfNull<T>(T? value) where T : struct => value ?? default(T);

        public class Event
        {
            public string display_html { get; set; }
            public string beatmap_id { get; set; }
            public string beatmapset_id { get; set; }
            public string date { get; set; }
            public string epicfactor { get; set; }
        }
    }
#pragma warning restore CS0649
}
