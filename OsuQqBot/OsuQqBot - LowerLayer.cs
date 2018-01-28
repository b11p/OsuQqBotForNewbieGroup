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
                    qq.SendMessageAsync(endPoint, "未绑定，请使用绑定<你的名字>命令绑定");
                }
                else
                {
                    Task.Run(async () =>
                    {
                        try
                        {
                            await SendQueryMessage(endPoint, uid.Value, message.Trim().Substring(1).Trim());
                        }
                        catch (Exception e)
                        {
                            Logger.LogException(e);
                        }
                    });
                }
            }
            else if (message.Trim().StartsWith("绑定"))
            {
                Task.Run(async () =>
                {
                    try
                    {
                        await BindAsync(endPoint, source.FromQq, message.Trim().Substring(2).Trim());
                    }
                    catch (Exception e)
                    {
                        Logger.LogException(e);
                    }
                });
            }
            else if (message.Trim().ToLowerInvariant().StartsWith("where"))
            {
                Task.Run(async () =>
                {
                    try
                    {
                        var (success, info) = await ProcessQuery(username: message.Trim().Substring("where".Length).Trim());
                        this.qq.SendMessageAsync(endPoint, this.qq.BeforeSend(info));
                    }
                    catch (Exception e)
                    {
                        Logger.LogException(e);
                    }
                });
            }
            else if (!string.IsNullOrWhiteSpace(message) && message.Split()[0] == "帮助")
            {
                if (message.Split().Length > 1) ShowHelp(endPoint, message.Split()[1]);
                else ShowHelp(endPoint);
            }
            else
            {
                bool done = false;
                switch (endPoint)
                {
                    case PrivateEndPoint p:
                        done = ProcessPrivateMessage(p, source, message);
                        if (!done)
                            Task.Run(() =>
                                done = PrivateStatefulFunctions(p, source, message));
                        break;
                    case GroupEndPoint g:
                        ProcessGroupMessage(g, source, message);
                        break;
                    case DiscussEndPoint d:
                        break;
                }
            }
        }

        public bool ProcessPrivateMessage(PrivateEndPoint endPoint, MessageSource source, string message)
        {
            return PrivateManage(endPoint.UserId, message);
        }

        public void ProcessGroupMessage(GroupEndPoint endPoint, MessageSource source, string message)
        {
            Task.Run(async () =>
            {
                try
                {
                    if (await UpdateUserBandingAsync(endPoint.GroupId, source.FromQq, message)) return;
                    if (await WhirIsBestAsync(endPoint.GroupId, source.FromQq, message)) return;
                    await TestInGroupNameAsync(endPoint.GroupId, source.FromQq, message);
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
        private static string At(long qq) => $"[CQ:at,qq={qq}]";

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