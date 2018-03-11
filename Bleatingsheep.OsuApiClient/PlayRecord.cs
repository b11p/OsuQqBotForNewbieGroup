using Newtonsoft.Json;
using System;

namespace Bleatingsheep.OsuMixedApi
{
    public class PlayRecord
    {
        public Mode Mode { get; internal set; }
        public double Accuracy
        {
            get
            {
                double accuracy;
                switch (Mode)
                {
                    case Mode.Standard:
                        accuracy = (50 * Count50 + 100 * Count100 + 300 * Count300)
                            / (double)((CountMiss + Count50 + Count100 + Count300) * 300);
                        break;
                    case Mode.Taiko:
                        accuracy = (Count100 * 150 + Count300 * 300)
                            / (double)((CountMiss + Count100 + Count300) * 300);
                        break;
                    case Mode.Ctb:
                        accuracy = (Count50 + Count100 + Count300)
                            / (double)(CountMiss + CountKatu + Count50 + Count100 + Count300);
                        break;
                    case Mode.Mania:
                        accuracy = (50 * Count50 + 100 * Count100 + 200 * CountKatu + 300 * Count300 + 300 * CountGeki)
                            / (double)((CountMiss + Count50 + Count100 + CountKatu + Count300 + CountGeki) * 300);
                        break;
                    default:
                        return 0;
                }
                return accuracy;
            }
        }

        [JsonProperty("beatmap_id")]
        public int Bid { get; private set; }
        [JsonProperty("score")]
        public long Score { get; private set; }
        [JsonProperty("maxcombo")]
        public int Combo { get; private set; }
        [JsonProperty("count50")]
        public int Count50 { get; private set; }
        [JsonProperty("count100")]
        public int Count100 { get; private set; }
        [JsonProperty("count300")]
        public int Count300 { get; private set; }
        [JsonProperty("countmiss")]
        public int CountMiss { get; private set; }
        /* Miss droplets in ctb
         * 200 in mania
         */
        [JsonProperty("countkatu")]
        public int CountKatu { get; private set; }
        [JsonProperty("countgeki")]
        public int CountGeki { get; private set; } // rainbow 300 in mania

        [JsonProperty("perfect")]
#pragma warning disable CS0649
        private int perfect;
#pragma warning restore CS0649
        [JsonIgnore]
        public bool Perfect => perfect != 0;

        [JsonProperty("enabled_mods")]
        public Mods Mods { get; private set; }
        [JsonProperty("user_id")]
        public int Uid { get; private set; }

        [JsonProperty("date")]
#pragma warning disable CS0649
        private DateTime date;
#pragma warning restore CS0649
        [JsonIgnore]
        public DateTimeOffset DateOffset => new DateTimeOffset(date, OsuApiClient.TimeZone);

        [JsonProperty("rank")]
        public string Rank { get; private set; }
    }
}
