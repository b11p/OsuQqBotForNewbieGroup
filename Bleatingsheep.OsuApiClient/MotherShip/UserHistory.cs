using Newtonsoft.Json;
using System;

namespace Bleatingsheep.OsuMixedApi.MotherShip
{
#pragma warning disable CS0649
    [JsonObject(MemberSerialization.OptIn)]
    public class UserHistory
    {
        [JsonProperty("username")]
        public string Username { get; private set; }
        [JsonProperty("mode")]
        public Mode Mode { get; private set; }
        [JsonProperty("userId")]
        public int Id { get; private set; }
        [JsonProperty("count300")]
        public int Count300 { get; set; }
        [JsonProperty("count100")]
        public int Count100 { get; set; }
        [JsonProperty("count50")]
        public int Count50 { get; set; }
        public int TotalHits => Count300 + Count100 + Count50;
        [JsonProperty("playcount")]
        public int PlayCount { get; private set; }
        [JsonProperty("accuracy")]
        private double _accuracy;
        public double Accuracy => _accuracy / 100.0;
        [JsonProperty("ppRaw")]
        public double PP { get; private set; }
        [JsonProperty("rankedScore")]
        public long RankedScore { get; private set; }
        [JsonProperty("totalScore")]
        public long TotalScore { get; private set; }
        [JsonProperty("level")]
        public double Level { get; private set; }
        [JsonProperty("ppRank")]
        public int Rank { get; private set; }
        [JsonProperty("countRankSs")]
        public int CountSs { get; private set; }
        [JsonProperty("countRankSsh")]
        public int CountSsh { get; private set; }
        [JsonProperty("countRankS")]
        public int CountS { get; private set; }
        [JsonProperty("countRankSh")]
        public int CountSh { get; private set; }
        [JsonProperty("countRankA")]
        public int CountA { get; private set; }
        [JsonProperty("queryDate")]
        public Querydate QueryDate { get; private set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Querydate
    {
        [JsonProperty("year")]
        public int Year { get; private set; }
        [JsonProperty("month")]
        public int Month { get; private set; }
        [JsonProperty("day")]
        public int Day { get; private set; }
        public DateTime Date => new DateTime(Year, Month, Day);
    }
#pragma warning restore CS0649
}
