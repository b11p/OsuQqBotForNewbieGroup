using Bleatingsheep.OsuMixedApi;
using OsuQqBot.Commands;
using Sisters.WudiLib.Posts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OsuQqBot.Charts
{
    static class ChartExecution
    {


        public static string Execute(Endpoint endpoint, MessageSource source, string param, out string location)
        {
            IReadOnlyList<string> lines = param.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
            string filtedParam = param.Trim();

            if (string.Compare("new", lines[0], true, CultureInfo.InvariantCulture) == 0)
            {

            }

            location = null;
            return "无法识别命令" + lines[0];
        }

        public static string New(IReadOnlyList<string> param, int index)
        {
            var chartInfo = ChartInfo.New(param, index);
        }
    }

    sealed class ChartInfo
    {
        private string _name;
        private string _description;
        DateTimeOffset? _offset;
        DateTime? _start;
        DateTime? _end;
        double? _ppRecommand;
        double? _ppMax;

        private ChartInfo() { }

        /// <summary>
        /// 通过命令创建 chart。
        /// </summary>
        /// <param name="param">创建 chart 的命令（第一行是“new”）。</param>
        /// <param name="index">创建 chart 的命令参数开始行数（“new”之后的一行）的下标。</param>
        /// <exception cref="IndexOutOfRangeException">只有“new”，没有后续参数。</exception>
        /// <returns></returns>
        public static ChartInfo New(IReadOnlyList<string> param, int index)
        {
            var info = new ChartInfo();
            info._name = param[index];

            for (int i = index + 1; i < param.Count; i++)
            {

            }
        }

        sealed class ChartBeatmapInfo
        {
            public int Bid { get; set; }
            public Mode Mode { get; set; }
        }
    }
}
