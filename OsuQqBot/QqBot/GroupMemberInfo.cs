using System;

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

    public static class GroupMemberInfoExtends
    {
        public static string InGroupOrNickname(this GroupMemberInfo groupMemberInfo)
        {
            if (groupMemberInfo == null) throw new ArgumentNullException(nameof(groupMemberInfo));

            if (string.IsNullOrEmpty(groupMemberInfo.InGroupName)) return groupMemberInfo.QqNickname;
            return groupMemberInfo.InGroupName;
        }
    }
}