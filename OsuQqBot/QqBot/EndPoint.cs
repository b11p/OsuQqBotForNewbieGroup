using System;
using System.Collections.Generic;
using System.Text;

namespace OsuQqBot.QqBot
{
    /// <summary>
    /// 表示要发送的目的地，或者接收的来源
    /// </summary>
    public abstract class EndPoint
    {
        public EndPointType EndPointType { get; set; }
    }

    public enum EndPointType
    {
        Unknown = 0,
        Private = 1,
        Group = 2,
        Discuss = 3
    }

    /// <summary>
    /// 表示要发送到的群，或者来自的群
    /// </summary>
    public class GroupEndPoint : EndPoint
    {
        public long GroupId { get; set; }
    }

    /// <summary>
    /// 表示要发送到的人，或者来自的人
    /// </summary>
    public class PrivateEndPoint : EndPoint
    {
        public long UserId { get; set; }
    }

    /// <summary>
    /// 表示要发送到的讨论组，或者来自的讨论组
    /// </summary>
    public class DiscussEndPoint : EndPoint
    {
        public long DiscussId { get; set; }
    }
}
