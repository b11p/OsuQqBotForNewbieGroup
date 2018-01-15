using System;
using System.Collections.Generic;

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
        /// Bot生效的群们
        /// </summary>
        readonly ISet<long> ValidGroups = new HashSet<long>();

        /// <summary>
        /// 主管理员ID
        /// </summary>
        readonly long id_Kamisama;

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
    }
}