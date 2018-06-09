using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OsuQqBot.QqBot;

namespace OsuQqBot
{
    public partial class OsuQqBot
    {
        bool checkSwitch = true;

        /// <summary>
        /// 新人群号
        /// </summary>
        readonly long GroupId;

        /// <summary>
        /// 开启新图提醒的群列表。
        /// </summary>
        readonly ISet<long> ValidGroups = new HashSet<long>();

        /// <summary>
        /// 主管理员ID
        /// </summary>
        readonly long id_Kamisama;

        /// <summary>
        /// 公开的主管理员ID
        /// </summary>
        public static long IdWhoLovesInt100Best { get; private set; }

        /// <summary>
        /// 接收私聊消息，管理bot
        /// </summary>
        /// <param name="qq"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public bool PrivateManage(long qq, string command)
        {
            if (qq != id_Kamisama) return false;
            const string startCheckPP = "开启PP检查";
            const string stopCheckPP = "停止PP检查";
            const string help = "帮助";
            switch (command)
            {
                case help:
                    this.qq.SendPrivateMessageAsync(qq, startCheckPP + Environment.NewLine + stopCheckPP);
                    return true;
                case startCheckPP:
                    checkSwitch = true;
                    this.qq.SendPrivateMessageAsync(qq, "成功开启");
                    return true;
                case stopCheckPP:
                    checkSwitch = false;
                    this.qq.SendPrivateMessageAsync(qq, "成功关闭");
                    return true;
                default:
                    break;
            }
            System.Text.RegularExpressions.Regex ignoreCommandRegex = new System.Text.RegularExpressions.Regex(
                @"^\s*ignore\s*(\d+)\s*$");
            var ignoreCommandMatch = ignoreCommandRegex.Match(command);
            if (ignoreCommandMatch.Success)
            {
                if (long.TryParse(ignoreCommandMatch.Groups[1].Value, out long ignoredId))
                {
                    var memberInfo = this.qq.GetGroupMemberInfo(GroupId, ignoredId);
                    if (memberInfo != null)
                    {
                        ignoreList.Add(ignoredId);
                        this.qq.SendPrivateMessageAsync(qq, $"忽略{memberInfo.InGroupName}");
                        SaveIgnoreList();
                    }
                }
                else this.qq.SendPrivateMessageAsync(qq, "Parse失败");
                return true;
            }

            System.Text.RegularExpressions.Regex ignorePPCommandRegex = new System.Text.RegularExpressions.Regex(
                @"^\s*ignore\s+pp\s*(\d+)\s*$");
            var ignorePPCommandMatch = ignorePPCommandRegex.Match(command);
            if (ignorePPCommandMatch.Success)
            {
                if (long.TryParse(ignorePPCommandMatch.Groups[1].Value, out long ignoredId))
                {
                    var memberInfo = this.qq.GetGroupMemberInfo(GroupId, ignoredId);
                    if (memberInfo != null)
                    {
                        ignorePPList.Add(ignoredId);
                        this.qq.SendPrivateMessageAsync(qq, $"忽略{memberInfo.InGroupName}的PP");
                        SaveIgnoreList();
                    }
                }
                else this.qq.SendPrivateMessageAsync(qq, "Parse失败");
                return true;
            }

            return false;
        }

        /// <summary>
        /// 从群里接收命令，显示未绑定的群员。
        /// </summary>
        /// <param name="group"></param>
        /// <param name="qq"></param>
        /// <returns></returns>
        private bool ListUnbind(long group, long qq, string message)
        {
            if (!database.IsAdministrator(qq)) return false;
            if (message != "列出未绑定的群友") return false;

            var members = this.qq.GetGroupMemberList(group);
            var ignore = ignoreList.Union(ignorePPList).Distinct();

            var unbind = members.Where(m => !ignore.Contains(m.Qq) && Query.Querying.Instance.GetUserBind(m.Qq).Result == 0);

            var sb = new StringBuilder();
            foreach (var item in unbind)
            {
                sb.AppendFormat("{0}({1})", item.InGroupOrNickname(), item.Qq);
                sb.AppendLine();
            }

            sb.Append("---end---");

            this.qq.SendGroupMessageAsync(group, sb.ToString(), true);

            return true;
        }
    }
}