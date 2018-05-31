namespace OsuQqBot
{
    sealed class Config
    {
        public long Kamisama { get; set; }
        public long MainGroup { get; set; }
        public long[] ValidGroups { get; set; }
        public string ApiKey { get; set; }
        public long Daloubot { get; set; }
        public string Int100ApiKey { get; set; }
    }
}
