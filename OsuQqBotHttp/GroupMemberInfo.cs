namespace OsuQqBotHttp
{
#pragma warning disable IDE1006
    internal class GroupMemberListResponse
    {
        public GroupMemberInfo[] data { get; set; }
        public int retcode { get; set; }
        public string status { get; set; }
    }

    public class GroupMemberInfoResponse
    {
        public GroupMemberInfo data { get; set; }
        public int retcode { get; set; }
        public string status { get; set; }
    }

    public class GroupMemberInfo
    {
        public int age { get; set; }
        public string area { get; set; }
        public string card { get; set; }
        public bool card_changeable { get; set; }
        public long group_id { get; set; }
        public int join_time { get; set; }
        public int last_sent_time { get; set; }
        public string level { get; set; }
        public string nickname { get; set; }
        public string role { get; set; }
        public string sex { get; set; }
        public string title { get; set; }
        public int title_expire_time { get; set; }
        public bool unfriendly { get; set; }
        public long user_id { get; set; }
#pragma warning restore IDE1006

        public static implicit operator OsuQqBot.QqBot.GroupMemberInfo(GroupMemberInfo info)
        {
            if (info == null) return null;
            var result = new OsuQqBot.QqBot.GroupMemberInfo
            {
                InGroupName = info.card,
                Qq = info.user_id,
                QqNickname = info.nickname,
            };
            switch (info.role)
            {
                case "owner":
                    result.Authority = OsuQqBot.QqBot.GroupMemberInfo.GroupMemberAuthority.Leader;
                    break;
                case "admin":
                    result.Authority = OsuQqBot.QqBot.GroupMemberInfo.GroupMemberAuthority.Manager;
                    break;
                case "member":
                    result.Authority = OsuQqBot.QqBot.GroupMemberInfo.GroupMemberAuthority.Normal;
                    break;
                default:
                    result.Authority = OsuQqBot.QqBot.GroupMemberInfo.GroupMemberAuthority.Unknown;
                    break;
            }
            return result;
        }
    }
}