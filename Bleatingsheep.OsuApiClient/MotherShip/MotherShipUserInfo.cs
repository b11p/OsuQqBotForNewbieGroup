using Newtonsoft.Json;
using System;

namespace Bleatingsheep.OsuMixedApi.MotherShip
{
    [JsonObject(MemberSerialization.OptIn)]
    public class MotherShipUserInfo
    {
#pragma warning disable CS0649
        [JsonProperty("userId")]
        public int OsuId { get; private set; }
        [JsonProperty("role")]
        private string role;
        public string[] Roles => role.Split(',', StringSplitOptions.RemoveEmptyEntries);
        [JsonProperty("qq")]
        public long QqId { get; private set; }
        [JsonProperty("legacyUname")]
        private string legacyName;
        public string[] LegacyNames => JsonConvert.DeserializeObject<string[]>(legacyName);
        [JsonProperty("currentUname")]
        public string Name { get; private set; }
        [JsonProperty("banned")]
        public bool IsBanned { get; private set; }
        //[JsonProperty("mode")]
        //public Mode Mode { get; private set; }
        [JsonProperty("repeatCount")]
        public int RepeatCount { get; private set; }
        [JsonProperty("speakingCount")]
        public int SpeakingCount { get; private set; }
#pragma warning restore CS0649
    }
}
