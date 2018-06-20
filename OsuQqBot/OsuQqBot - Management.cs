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
    }
}