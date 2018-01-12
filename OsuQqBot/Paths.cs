using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OsuQqBot
{
    /// <summary>
    /// 用于存储各种路径的静态类
    /// </summary>
    static class Paths
    {
        /// <summary>
        /// Bot 数据存储根目录
        /// </summary>
        internal static string DataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Sheep Bot Data");

        ///// <summary>
        ///// 存储 QQ 与 osu!ID 的绑定数据
        ///// </summary>
        //internal static string BindPath => Path.Combine(DataPath, "Binding Data");

        ///// <summary>
        ///// 存储昵称的目录
        ///// </summary>
        //internal static string NicknamePath => Path.Combine(DataPath, "nicknames");

        /// <summary>
        /// json配置文件所在路径
        /// </summary>
        internal static string JsonConfigPath => Path.Combine(DataPath, "config.json");

        internal static string IgnoreListPath => Path.Combine(DataPath, "ignore.txt");
    }
}
