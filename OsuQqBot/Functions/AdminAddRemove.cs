using System;
using System.Collections.Generic;
using System.Text;
using OsuQqBot.QqBot;

namespace OsuQqBot.Functions
{
    /// <summary>
    /// 添加或者删除管理员的功能
    /// </summary>
    class AdminAddRemove : IFunction
    {
        /// <summary>
        /// 指示是否正在处理中。（用此变量标记是否还要接收后续命令）
        /// </summary>
        bool inProcess;

        /// <summary>
        /// 根据是否处理返回应有的IFunction
        /// </summary>
        private IFunction StateFunction => inProcess ? this : null;

        private object processLock = new object();

        /// <summary>
        /// 获取提示当前路径的信息
        /// </summary>
        private string CurrentHint => inProcess ? "(admin)#" : ">";

        public (bool handled, IFunction state) ProcessMessage(EndPoint endPoint, MessageSource messageSource, string message)
        {
            if (!(endPoint is PrivateEndPoint)) return (false, null);

            if (!HasPower(messageSource.FromQq))
            {
                // 如果没有权限，就按是否处在处理中返回
                if (inProcess) OsuQqBot.QqApi.SendMessageAsync(endPoint, OsuQqBot.QqApi.BeforeSend("失去管理权限"));
                return (inProcess, null);
            }
            else
            {
                string result = string.Empty;
                var comm = message.Split();
                bool handled = false; // 已经接受并处理（会返回消息）
                if (!inProcess)
                    // 初次收到消息。
                    if (comm[0].ToLowerInvariant() == "admin") // "".Split() // { "" }
                    {
                        if (comm.Length > 1)
                        {// 一次性命令
                            string[] inner = new string[comm.Length - 1]; // 要执行的命令
                            for (int i = 1; i < comm.Length; i++)
                            {
                                inner[i - 1] = comm[i];
                            }
                            result = DoCommond(inner);
                            handled = true;
                        }
                        else
                        {// 只进入管理模式
                            result = "";
                            inProcess = true;
                            handled = true;
                        }
                    }
                    else handled = false;
                else
                {
                    // 不是初次收到消息
                    result = DoCommond(message.Split());
                    handled = result != null;

                }
                if (handled) result += (result == string.Empty ? "" : Environment.NewLine) +
                        CurrentHint;
                if (handled) OsuQqBot.QqApi.SendMessageAsync(endPoint, result);
                return (handled, StateFunction);

            }
        }

        private string DoCommond(string[] commonds)
        {
            string result;
            if (commonds.Length == 1)
            {
                switch (commonds[0].ToLowerInvariant())
                {
                    case "help":
                        result =
                            @"add <qq> 添加管理员
revoke <qq> 删除管理员
list 列出管理员
help 显示帮助
bye 退出";
                        break;
                    case "bye":
                        result = "";
                        inProcess = false;
                        break;
                    case "list":
                        result = "no support for listing administrators";
                        break;
                    default:
                        result = null;
                        break;
                }
                return result;
            }
            else if (commonds.Length != 2)
                return null;

            // 普通的两参数命令
            long operatedQq; // 被操作的QQ号
            switch (commonds[0].ToLowerInvariant())
            {
                case "add":
                    if (long.TryParse(commonds[1], out operatedQq))
                    {
                        result = LocalData.Database.Instance.GiveAdministrator(operatedQq) ? "添加成功" : "已经存在";
                    }
                    else result = "请输入正确的命令";
                    break;
                case "revoke":
                    if (long.TryParse(commonds[1], out operatedQq))
                    {
                        result = LocalData.Database.Instance.RevokeAdministrator(operatedQq) ? "收回权限成功" : "此人不是管理员";
                    }
                    else result = "请输入正确的命令";
                    break;
                default:
                    return null;
            }
            return result;

        }

        private static bool HasPower(long qq) => qq == OsuQqBot.IdWhoLovesInt100Best;
    }
}
