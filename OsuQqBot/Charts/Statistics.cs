using Bleatingsheep.OsuQqBot.Database;
using Bleatingsheep.OsuQqBot.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
            
        }
        
        private static readonly IReadOnlyDictionary<string, (Action<Stack<double>> function, int priority)> Operators
            = new Dictionary<string, (Action<Stack<double>> function, int priority)>
            {
                { "+", ( s => s.Push( s.Pop() + s.Pop()), 1) },
                { "-", ( s => s.Push( -s.Pop() + s.Pop()), 1) },
                { "*", ( s => s.Push( s.Pop() * s.Pop()), 2) },
                { "/", ( s => { double y = s.Pop(), x = s.Pop(); s.Push(x / y); }, 2) },
                { "^", ( s => { double y = s.Pop(), x = s.Pop(); s.Push(Math.Pow(x, y)); }, 3) },
            };
    }
}
