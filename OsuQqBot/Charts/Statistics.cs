using Bleatingsheep.OsuQqBot.Database;
using Bleatingsheep.OsuQqBot.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OsuQqBot.Charts
{
    static class Statistics
    {
        public static async Task<(string title, IEnumerable<(int uid, double score)> rank)> RankAsync(int chartId)
        {
            var chart = await NewbieDatabase.GetChartWithCommitsAsync(chartId);
            if (chart == null) return (null, null);

            var maps = chart.Maps.Select(b => (beatmap: b, score: ParseScorer(b.ScoreCalculation)));

            var results =
                from m in maps
                from c in m.beatmap.Commits
                let calcRawScore = m.score(c)
                let score = double.IsNaN(calcRawScore) ? 0 : calcRawScore
                group new { commit = c, score } by new { c.Beatmap, c.Uid } into commits // Key 是 map 和 uid，Values 是 Commits。
                group commits.Max(cm => cm.score) by commits.Key.Uid into highScores // Key 是 uid，Values 是每个图的最高分。
                //select (uid: highScores.Key, score: highScores.Sum()) into r
                let r = (uid: highScores.Key, score: highScores.Sum())
                orderby r.score descending
                select r;

            return (chart.ChartName, results);
        }

        /// <summary>
        /// 将分数表达式转换为计算委托。
        /// </summary>
        /// <param name="scoreCalculation"></param>
        /// <returns></returns>
        private static Func<ChartCommit, double> ParseScorer(string scoreCalculation)
        {
            if (string.IsNullOrWhiteSpace(scoreCalculation)) return s => s.Score;

            throw new NotImplementedException("未实现自定义公式计分。");

#pragma warning disable CS0162 // 检测到无法访问的代码
            var data = ProcessingData.Create();
#pragma warning restore CS0162 // 检测到无法访问的代码
            ProcessExpression(scoreCalculation, data);
        }

        /// <summary>
        /// 处理表达式。
        /// </summary>
        /// <param name="scoreCalculation"></param>
        /// <param name="data"></param>
        /// <param name="index"></param>
        private static void ProcessExpression(string scoreCalculation, ProcessingData data, int index = 0)
        {
            // want: digits or "("


        }

        private static double? NextNumberOrLeftParenthese(string scoreCalculation, ProcessingData data, int index)
        {
            throw new NotImplementedException("尚未实现读取数字。");
        }

        /// <summary>
        /// 从指定索引开始读取下一项。
        /// </summary>
        /// <param name="scoreCalculation"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private static string ReadNext(string scoreCalculation, int index)
        {
            throw new NotImplementedException("尚未实现读取下一项。");
        }

        private class ProcessingData
        {
            public Stack<char> Stack;
            public LinkedList<string> Expression;

            public static ProcessingData Create()
            {
                var result = new ProcessingData
                {
                    Stack = new Stack<char>(),
                    Expression = new LinkedList<string>()
                };
                return result;
            }
        }

        private static readonly IReadOnlyDictionary<string, Action<ChartCommit, Stack<double>>> Operators
            = new Dictionary<string, Action<ChartCommit, Stack<double>>>
            {
                { "+", (c, s) => s.Push( s.Pop() + s.Pop()) },
                { "-", (c, s) => s.Push( -s.Pop() + s.Pop()) },
                { "*", (c, s) => s.Push( s.Pop() * s.Pop()) },
                { "/", (c, s) => { double y = s.Pop(), x = s.Pop(); s.Push(x / y); } },
                { "^", (c, s) => { double y = s.Pop(), x = s.Pop(); s.Push(Math.Pow(x, y)); } },
            };
    }
}
