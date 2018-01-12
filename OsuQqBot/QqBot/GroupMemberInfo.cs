namespace OsuQqBot.QqBot
{
    public sealed class GroupMemberInfo
    {
        public GroupMemberAuthority Authority { get; set; }
        public long Qq { get; set; }
        public string QqNickname { get; set; }
        public string InGroupName { get; set; }


        public enum GroupMemberAuthority
        {
            Unknown = 0,
            Normal = 1,
            Manager = 2,
            Leader = 3
        }
    }
}