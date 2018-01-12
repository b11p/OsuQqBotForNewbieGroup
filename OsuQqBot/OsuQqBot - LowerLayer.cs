using OsuQqBot.QqBot;
using System;
using System.Threading.Tasks;

namespace OsuQqBot
{
    public partial class OsuQqBot
    {
        public void ProcessMessage(EndPoint endPoint, MessageSource source, string message)
        {
            if (message.Trim().StartsWith("~") || message.Trim().StartsWith("～"))
            {
                var uid = FindUid(source.FromQq).Result;
                if (uid == null)
                {
                    qq.SendMessageAsync(endPoint, "网络异常");
                }
                else if (uid.Value == 0)
                {
                    qq.SendMessageAsync(endPoint, "未绑定");
                }
                else
                {
                    Query(endPoint, uid.Value, message.Trim().Substring(1).Trim());
                }
            }
        }

        public void ProcessPrivateMessage(PrivateEndPoint endPoint, MessageSource source, string message)
        {

        }

        public void ProcessGroupMessage(GroupEndPoint endPoint, MessageSource source, string message)
        {
            Task.Run(() =>
            {
                try
                {
                    if (UpdateUserBandingAsync(endPoint.GroupId, source.FromQq, message).Result) return;
                    if (WhirIsBest(endPoint.GroupId, source.FromQq, message)) return;
                    TestInGroupNameAsync(endPoint.GroupId, source.FromQq, message).Wait();
                }
                catch (Exception e)
                {
                    Logger.LogException(e);
                }
            });
        }

        /// <summary>
        /// 获取群内At代码
        /// </summary>
        /// <param name="qq"></param>
        /// <returns></returns>
        private static string At(long qq) => $"[CQ:at,qq={qq}] ";

        /// <summary>
        /// 获取群名片
        /// </summary>
        /// <param name="fromGroup"></param>
        /// <param name="fromQq"></param>
        /// <returns>如果失败返回null</returns>
        private string GetInGroupName(long fromGroup, long fromQq)
        {
            string inGroupName;
            var memberInfo = qq.GetGroupMemberInfo(fromGroup, fromQq);
            if (memberInfo == null)
            {
                return null;
            }
            inGroupName = memberInfo.InGroupName;
            if (string.IsNullOrEmpty(inGroupName))
                inGroupName = memberInfo.QqNickname;
            return inGroupName;
        }
    }
}