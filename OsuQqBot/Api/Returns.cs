#pragma warning disable IDE1006
namespace OsuQqBot.Api
{
    class UserRaw
    {
        public string user_id { get; set; }
        public string username { get; set; }
        public string count300 { get; set; }
        public string count100 { get; set; }
        public string count50 { get; set; }
        public string playcount { get; set; }
        public string ranked_score { get; set; }
        public string total_score { get; set; }
        public string pp_rank { get; set; }
        public string level { get; set; }
        public string pp_raw { get; set; }
        public string accuracy { get; set; }
        public string count_rank_ss { get; set; }
        //public string count_rank_ssh { get; set; }
        public string count_rank_s { get; set; }
        //public string count_rank_sh { get; set; }
        public string count_rank_a { get; set; }
        public string country { get; set; }
        public string pp_country_rank { get; set; }
        public Event[] events { get; set; }
    }

    class Event
    {
        public string display_html { get; set; }
        public string beatmap_id { get; set; }
        public string beatmapset_id { get; set; }
        public string date { get; set; }
        public string epicfactor { get; set; }
    }

    public class best_performance
    {
        public string beatmap_id { get; set; }
        public string score { get; set; }
        public string maxcombo { get; set; }
        public string count300 { get; set; }
        public string count100 { get; set; }
        public string count50 { get; set; }
        public string countmiss { get; set; }
        public string countkatu { get; set; }
        public string countgeki { get; set; }
        public string perfect { get; set; }
        public string enabled_mods { get; set; }
        public string user_id { get; set; }
        public string date { get; set; }
        public string rank { get; set; }
        public string pp { get; set; }
    }
    
    class beatmap
    {
        public string beatmapset_id { get; set; }
        public string beatmap_id { get; set; }
        public string approved { get; set; }
        public string total_length { get; set; }
        public string hit_length { get; set; }
        public string version { get; set; }
        public string file_md5 { get; set; }
        public string diff_size { get; set; }
        public string diff_overall { get; set; }
        public string diff_approach { get; set; }
        public string diff_drain { get; set; }
        public string mode { get; set; }
        public string approved_date { get; set; }
        public string last_update { get; set; }
        public string artist { get; set; }
        public string title { get; set; }
        public string creator { get; set; }
        public string bpm { get; set; }
        public string source { get; set; }
        public string tags { get; set; }
        public string genre_id { get; set; }
        public string language_id { get; set; }
        public string favourite_count { get; set; }
        public string playcount { get; set; }
        public string passcount { get; set; }
        public string max_combo { get; set; }
        public string difficultyrating { get; set; }
    }
}
#pragma warning restore IDE1006