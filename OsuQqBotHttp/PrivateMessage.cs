#pragma warning disable IDE1006
namespace OsuQqBotHttp
{
    public class Post
    {
        public string post_type { get; set; }
        public int time { get; set; }
    }

    public class Message : Post
    {
        public string message_type { get; set; }
    }

    public sealed class PrivateMessage : Message
    {
        public int font { get; set; }
        public string message { get; set; }
        public int message_id { get; set; }
        public string sub_type { get; set; }
        public long user_id { get; set; }
    }

    public sealed class GroupMessage : Message
    {
        public string anonymous { get; set; }
        public string anonymous_flag { get; set; }
        public int font { get; set; }
        public long group_id { get; set; }
        public string message { get; set; }
        public int message_id { get; set; }
        public string sub_type { get; set; }
        public long user_id { get; set; }
    }

    public sealed class DiscussMessage : Message
    {
        public int discuss_id { get; set; }
        public int font { get; set; }
        public string message { get; set; }
        public int message_id { get; set; }
        public int user_id { get; set; }
    }
}
#pragma warning restore IDE1006