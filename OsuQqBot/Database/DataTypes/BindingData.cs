using System;
using System.Collections.Generic;
using System.Text;

namespace OsuQqBot.LocalData.DataTypes
{
    /// <summary>
    /// 一个QQ绑定的osu!账号的数据
    /// </summary>
    sealed class BindingData
    {
        public BindingData(long uid, string source)
        {
            Uid = uid;
            Source = source;
        }
        public long Uid { get; set; }
        public string Source { get; set; }
    }
}
