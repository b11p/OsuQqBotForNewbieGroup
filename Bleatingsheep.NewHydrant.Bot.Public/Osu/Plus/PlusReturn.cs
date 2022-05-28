namespace Bleatingsheep.NewHydrant.Osu.Plus
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using System.Runtime.Serialization;
    using Bleatingsheep.Osu;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class PlusUserReturn
    {
        [JsonProperty("user_data")]
        public PlusUserData Data { get; set; }

        [JsonProperty("user_performances")]
        public PlusUserPerformances Performances { get; set; }
    }

    public class PlusUserData
    {
        [JsonProperty("CountRankS")]
        public int CountRankS { get; set; }

        [JsonProperty("PrecisionTotal")]
        public double Precision { get; set; }

        [JsonProperty("AccuracyPercentTotal")]
        public double AccuracyPercent { get; set; }

        [JsonProperty("AccuracyTotal")]
        public double Accuracy { get; set; }

        [JsonProperty("PerformanceTotal")]
        public double Performance { get; set; }

        [JsonProperty("Rank")]
        public int Rank { get; set; }

        [JsonProperty("SpeedTotal")]
        public double Speed { get; set; }

        [JsonProperty("CountryCode")]
        public string CountryCode { get; set; }

        [JsonProperty("UserID")]
        public int UserId { get; set; }

        [JsonProperty("StaminaTotal")]
        public double Stamina { get; set; }

        [JsonProperty("AimTotal")]
        public double AimTotal { get; set; }

        [JsonProperty("CountryRank")]
        public int CountryRank { get; set; }

        [JsonProperty("JumpAimTotal")]
        public double AimJump { get; set; }

        [JsonProperty("PlayCount")]
        public int PlayCount { get; set; }

        [JsonProperty("CountRankSS")]
        public int CountRankSs { get; set; }

        [JsonProperty("UserName")]
        public string UserName { get; set; }

        [JsonProperty("FlowAimTotal")]
        public double AimFlow { get; set; }
    }

    public class PlusUserPerformances
    {
        [JsonProperty("stamina")]
        public Record[] Stamina { get; set; }

        [JsonProperty("accuracy")]
        public Record[] Accuracy { get; set; }

        [JsonProperty("precision")]
        public Record[] Precision { get; set; }

        [JsonProperty("speed")]
        public Record[] Speed { get; set; }

        [JsonProperty("aim")]
        public Record[] Aim { get; set; }

        [JsonProperty("flow_aim")]
        public Record[] FlowAim { get; set; }

        [JsonProperty("total")]
        public Record[] Total { get; set; }

        [JsonProperty("jump_aim")]
        public Record[] JumpAim { get; set; }
    }

    public class Record
    {
        [JsonProperty("SetID")]
        public int SetId { get; set; }

        [JsonProperty("Misses")]
        public int Misses { get; set; }

        [JsonProperty("Aim")]
        public double Aim { get; set; }

        [JsonProperty("BeatmapID")]
        public int BeatmapId { get; set; }

        [JsonProperty("Total")]
        public double Performance { get; set; }

        [JsonProperty("Combo")]
        public int Combo { get; set; }

        [JsonProperty("EnabledMods")]
        public Mods Mods { get; set; }

        [JsonProperty("Artist")]
        public string Artist { get; set; }

        [JsonProperty("HigherSpeed")]
        public double HigherSpeed { get; set; }

        [JsonProperty("Title")]
        public string Title { get; set; }

        [JsonProperty("MaxCombo")]
        public int MaxCombo { get; set; }

        [JsonProperty("Rank")]
        public string Rank { get; set; }

        [JsonProperty("Date")]
        public DateTimeOffset Date { get; set; }

        [JsonProperty("Speed")]
        public double Speed { get; set; }

        [JsonProperty("Stamina")]
        public double Stamina { get; set; }

        [JsonProperty("Count300")]
        public int Count300 { get; set; }

        [JsonProperty("Count100")]
        public int Count100 { get; set; }

        [JsonProperty("Precision")]
        public double Precision { get; set; }

        [JsonProperty("Count50")]
        public int Count50 { get; set; }

        [JsonProperty("Accuracy")]
        public double Accuracy { get; set; }

        [JsonProperty("UserID")]
        public int UserId { get; set; }

        [JsonProperty("JumpAim")]
        public double AimJump { get; set; }

        [JsonProperty("FlowAim")]
        public double AimFlow { get; set; }

        [JsonProperty("Version")]
        public string DifficultyName { get; set; }

        [JsonProperty("AccuracyPercent")]
        public double AccuracyPercent { get; set; }
    }

}
