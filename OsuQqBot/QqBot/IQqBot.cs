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
        void SendDiscussMessageAsync(long discuss, string message);
        void SendDiscussMessageAsync(long discuss, string message, bool isPlainText);

        void SendMessageAsync(EndPoint endPoint, string message);
        void SendMessageAsync(EndPoint endPoint, string message, bool isPlainText);

        /// <summary>
        /// 转义非功能部分
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        string BeforeSend(string message);

        /// <summary>
        /// 转换出原始消息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        string AfterReceive(string message);

        string At(long qq);

        string LocalImage(string path);

        string OnlineImage(string url);

        string OnlineImage(string url, bool noCache);

        event GroupAdminChangeEventHandler GroupAdminChange;
    }
}
