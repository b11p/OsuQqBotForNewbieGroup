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
        /// <exception cref="FormatException"></exception>
        private static void ProcessExpression(string scoreCalculation, ProcessingData data)
        {
            int index = 0;
            ProcessExpression(scoreCalculation, data, ref index);

            if (index < scoreCalculation.Length) throw new FormatException("Analyzing error. Most possibly you lost an open parenthesis in the beginning.");

            while (data.Stack.TryPop(out string top))
            {
                data.Expression.AddLast(top);
            }
        }
        private static void ProcessExpression(string scoreCalculation, ProcessingData data, ref int index)
        {
            while (index < scoreCalculation.Length)
            {
                string next;

                // want: value or "("
                next = NextValueOrOpenParenthesis(scoreCalculation, ref index);

                // 括号
                if (next == null) ProcessExpression(scoreCalculation, data, ref index);
                // 数字入表达式
                else data.Expression.AddLast(next);

                // want: ")" or symbol or end
                next = NextSymbolOrEnd(scoreCalculation, ref index);

                // 括号或结束
                if (next == null) return;
                // 入栈
                else data.Expression.AddLast(next);

                // want: value or "("
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scoreCalculation"></param>
        /// <param name="data"></param>
        /// <param name="index"></param>
        /// <exception cref="FormatException">下一项不是数字、变量或左括号，或者读取已经结束。</exception>
        /// <returns>该数字或变量；如果为左括号，则为 <c>null</c>。</returns>
        private static string NextValueOrOpenParenthesis(string scoreCalculation, ref int index)
        {
            int oldIndex = index;

            string next = ReadNext(scoreCalculation, ref index);

            if (string.IsNullOrEmpty(next)) throw new FormatException("The expression ends, but we expect a number, a variable or an open parenthesis.");

            if (next.Equals("(", StringComparison.Ordinal)) return null;

            return IsValue(next) ? next : throw new FormatException($"Unexpected symbol at {oldIndex}.");
        }

        // 转换出问题的时候考虑改用下面的方法。
        // private static bool IsOkayForValuePosition(string subExpression) => subExpression == "(" || !string.IsNullOrEmpty(subExpression) && IsValue(subExpression);

        private static bool IsValue(string next) => char.IsLetterOrDigit(next[0]) || next[0] == '.' || next[0] == '_';

        /// <summary>
        /// 读下一个符号，或者后括号，或者结束。
        /// </summary>
        /// <exception cref="FormatException">读到了值或者前括号。</exception>
        /// <returns>如果是后括号或者结束，<c>null</c>；否则，下一个符号。</returns>
        private static string NextSymbolOrEnd(string scoreCalculation, ref int index)
        {
            int oldIndex = index;

            string next = ReadNext(scoreCalculation, ref index);

            if (string.IsNullOrEmpty(next) || next == ")") return null;

            return !(IsValue(next) || next == "(") ? next : throw new FormatException($"Unexpected number or open parenthesis at {oldIndex}.");
        }

        /// <summary>
        /// 从指定索引开始读取下一项。
        /// </summary>
        /// <exception cref="FormatException"></exception>
        /// <returns>如果是末尾，则返回<c>null</c>；否则返回下一项。</returns>
        private static string ReadNext(string scoreCalculation, ref int index)
        {
            index = SkipWhiteSpace(scoreCalculation, index);

            if (index >= scoreCalculation.Length) return null;

            var match = Regex.Match(scoreCalculation.Substring(index),
                @"^(?:" +
                @"(?:[\d]+\.?|\.)[\d]*|" /* 是数字的情况 */ +
                @"\w+|" /* 字母 */ +
                @"\(|\)|" /* 左右括号 */ +
                @"[^\w\(\)\s]+" /* 其他 */ +
                @")", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            if (!match.Success) throw new FormatException($"Invalid expression. First error is at {index}.");

            index += match.Value.Length;
            return match.Value;
        }

        private static int SkipWhiteSpace(string s, int index)
        {
            while (index < s.Length && char.IsWhiteSpace(s[index]))
            {
                index++;
            }

            return index;
        }

        private class ProcessingData
        {
            public Stack<string> Stack { get; private set; }
            public LinkedList<string> Expression { get; private set; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="symbol"></param>
            /// <exception cref="FormatException"><c>symbol</c> 不是合法的运算符。</exception>
            public void SmartPush(string symbol)
            {
                if (!Operators.TryGetValue(symbol, out var oInfo)) throw new FormatException($"Invalid operator {symbol}.");
                int newPriority = oInfo.priority;
                while (Stack.TryPeek(out string top) && Operators[top].priority >= newPriority)
                {
                    Stack.Pop();
                    Expression.AddLast(top);
                }
            }

            public static ProcessingData Create()
            {
                var result = new ProcessingData
                {
                    Stack = new Stack<string>(),
                    Expression = new LinkedList<string>()
                };
                return result;
            }
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
