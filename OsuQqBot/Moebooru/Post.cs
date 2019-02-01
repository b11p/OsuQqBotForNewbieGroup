using Newtonsoft.Json;

#pragma warning disable IDE1006 // 命名样式
namespace OsuQqBot.Moebooru
{

    public class Post
    {
        private const string ProtocolPrefix = "https:";

        public int id { get; set; }
        public string tags { get; set; }
        public int created_at { get; set; }
        public int creator_id { get; set; }
        public string author { get; set; }
        public int change { get; set; }
        public string source { get; set; }
        public int score { get; set; }
        public string md5 { get; set; }
        public int file_size { get; set; }
        [JsonProperty]
        private string file_url { get; set; }
        [JsonIgnore]
        public string FileUrl => UrlFormat(file_url);
        public bool is_shown_in_index { get; set; }
        [JsonProperty]
        private string preview_url { get; set; }
        [JsonIgnore]
        public string PreviewUrl => UrlFormat(preview_url);
        public int preview_width { get; set; }
        public int preview_height { get; set; }
        public int actual_preview_width { get; set; }
        public int actual_preview_height { get; set; }
        [JsonProperty]
        private string sample_url { get; set; }
        [JsonIgnore]
        public string SampleUrl => UrlFormat(sample_url);
        public int sample_width { get; set; }
        public int sample_height { get; set; }
        public int sample_file_size { get; set; }
        [JsonProperty]
        private string jpeg_url { get; set; }
        [JsonIgnore]
        public string JpegUrl => UrlFormat(jpeg_url);
        public int jpeg_width { get; set; }
        public int jpeg_height { get; set; }
        public int jpeg_file_size { get; set; }
        public string rating { get; set; }
        public bool has_children { get; set; }
        public object parent_id { get; set; }
        public string status { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public bool is_held { get; set; }
        public string frames_pending_string { get; set; }
        public object[] frames_pending { get; set; }
        public string frames_string { get; set; }
        public object[] frames { get; set; }
        public object flag_detail { get; set; }

        public int JpegSizeFallback => jpeg_file_size == 0 ? file_size : jpeg_file_size;

        private static string UrlFormat(string ori)
        {
            if (ori.StartsWith("//")) return ProtocolPrefix + ori;
            return ori;
        }
    }
}
#pragma warning restore IDE1006 // 命名样式