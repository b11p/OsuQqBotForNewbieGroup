using System;

namespace OsuQqBot.QqBot
{
    public delegate void GroupAdminChangeEventHandler(IQqBot sender, GroupAdminChangeEventArgs e);

    public delegate void GroupNoticeEventHandler(IQqBot sender, GroupNoticeEventArgs e);

    public abstract class QqEventArgs : EventArgs
    {
        public DateTime Time { get; set; }
        public bool Handled { get; set; }
    }

    public sealed class GroupAdminChangeEventArgs : QqEventArgs
    {
        public GroupAdminChangeType Type { get; set; }
        public long GroupId { get; set; }
        public long UserId { get; set; }

        public enum GroupAdminChangeType
        {
            Set,
            Unset,
        }
    }

    public class GroupNoticeEventArgs : QqEventArgs
    {
        public long GroupId { get; }
        public string Info { get; }
    }
}