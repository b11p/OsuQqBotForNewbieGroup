using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OsuQqBot.QqBot;

namespace OsuQqBot
{
    public partial class OsuQqBot
    {
        /// <summary>
        /// 新人群号
        /// </summary>
        readonly long GroupId;

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