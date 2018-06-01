using Bleatingsheep.OsuMixedApi;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace OsuQqBot.Charts
{
    /// <summary>
    /// 表示处理 Chart 命令时更改的信息的类。
    /// </summary>
    internal sealed class ChartInfo
    {
        private string _name;
        private bool IsNew => _chartId == default(int);
        private readonly int _chartId;
        private readonly ChartInfoField<string> _description = new StringInfo();
        private readonly ChartInfoField<DateTime> _start = new DateTimeInfo();
        private readonly ChartInfoField<DateTime?> _end = new NullableDateTimeInfo();
        private readonly ChartInfoField<double> _ppRecommand = new DoubleInfo();
        private readonly ChartInfoField<double?> _ppMax = new NullableDoubleInfo();
        private readonly IList<ChartBeatmapInfo> _beatmaps = new List<ChartBeatmapInfo>();
        private ChartBeatmapInfo _currentBeatmap = null;

        private ChartInfo(int chartId = default(int))
        {
            _chartId = chartId;

        }

        private static readonly IReadOnlyDictionary<string, Func<ChartInfo, ChartInfoField>> s_ops;
        private static readonly TimeSpan s_offset = new TimeSpan(8, 0, 0);

        static ChartInfo()
        {
            var dictionary = new Dictionary<string, Func<ChartInfo, ChartInfoField>>();
            dictionary.Add("DESC", ci => ci._description);
            dictionary.Add("DESCRIPTION", dictionary["DESC"]);
            dictionary.Add("START", ci => ci._start);
            dictionary.Add("END", ci => ci._end);
            s_ops = dictionary;
        }

        /// <summary>
        /// 通过命令创建 chart。
        /// </summary>
        /// <param name="param">创建 chart 的命令（第一行是“new”）。</param>
        /// <param name="index">创建 chart 的命令参数开始行数（“new”之后的一行）的下标。</param>
        /// <exception cref="IndexOutOfRangeException">只有“new”，没有后续参数。</exception>
        /// <exception cref="FormatException"></exception>
        /// <returns></returns>
        public static ChartInfo New(IReadOnlyList<string> param, int index)
        {
            var info = new ChartInfo();
            info._name = param[index];
            index++;
            var (field, value, no) = ParseLine(param[index]);
            if (s_ops[field](info) != info._description) throw new FormatException(param[index]);
            index++;
            info.ParseInfo(param, index);
            return info;
        }

        /// <summary>
        /// 通过命令修改 chart。
        /// </summary>
        /// <param name="args">修改 chart 的命令（第一行是chart编号）。</param>
        /// <param name="index">修改 chart 的命令参数开始行数（chart编号之后的一行）的下标。</param>
        /// 
        /// <returns></returns>
        public static ChartInfo Modify(IReadOnlyList<string> args, int index)
        {
            if (!int.TryParse(args[0], out int chartId) || chartId == default(int))
            {
                throw new ArgumentException("Chart编号不正确。");
            }
            var info = new ChartInfo(chartId);
            info.ParseInfo(args, index);
            return info;
        }

        private const string LinePattern = @"^\s*(NO\s+)?(\S+)\s*(.*?)\s*$";
        private static readonly Regex Regex = new Regex(LinePattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <exception cref="FormatException"></exception>
        /// <returns></returns>
        private static (string field, string value, bool no) ParseLine(string line)
        {
            var match = Regex.Match(line);
            if (!match.Success) throw new FormatException(line);
            return (match.Groups[2].Value, match.Groups[3].Value, string.IsNullOrEmpty(match.Groups[0].Value));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <param name="index">第一个描述信息的下标。</param>
        /// <exception cref="FormatException"></exception>
        private void ParseInfo(IReadOnlyList<string> args, int index)
        {
            throw new NotImplementedException();

            for (int i = index; i < args.Count; i++)
            {

            }
        }

        private void SetInfo(string field, string value)
        {

        }

        sealed class ChartBeatmapInfo
        {
            public int Bid { get; set; }
            public Mode Mode { get; set; }
        }
    }
}
