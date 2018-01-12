using System.Collections.Generic;

namespace OsuQqBot.QqBot
{
    public interface IQqBot
    {
        long GetLoginQq();
        string GetLoginName();
        GroupMemberInfo GetGroupMemberInfo(long group, long qq);
        IEnumerable<GroupMemberInfo> GetGroupMemberList(long group);

        void SendPrivateMessageAsync(long qq, string message);
        void SendPrivateMessageAsync(long qq, string message, bool isPlainText);
        void SendGroupMessageAsync(long group, string message);
        void SendGroupMessageAsync(long group, string message, bool isPlainText);

        void SendMessageAsync(EndPoint endPoint, string message);
        void SendMessageAsync(EndPoint endPoint, string message, bool isPlainText);
    }
}
