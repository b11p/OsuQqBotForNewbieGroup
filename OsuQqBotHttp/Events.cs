using OsuQqBot.QqBot;
using System;

#pragma warning disable IDE1006
namespace OsuQqBotHttp
{
    public class Event : Post
    {
        [Newtonsoft.Json.JsonProperty("event")]
        public string _event { get; set; }
    }

    public class GroupAdminChanged : Event
    {
        public long group_id { get; set; }
        public string sub_type { get; set; }
        public long user_id { get; set; }

        public GroupAdminChangeEventArgs ToGroupAdminChangeEventArgs()
        {
            var args = new GroupAdminChangeEventArgs();
            args.Time = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(this.time);
            args.GroupId = this.group_id;
            args.UserId = this.user_id;
            switch (this.sub_type)
            {
                case "set":
                    args.Type = GroupAdminChangeEventArgs.GroupAdminChangeType.Set;
                    break;
                case "unset":
                    args.Type = GroupAdminChangeEventArgs.GroupAdminChangeType.Unset;
                    break;
                default:
                    throw new ArgumentException("子类型错误", nameof(this.sub_type));
            }
            return args;
        }
    }
}
#pragma warning restore IDE1006