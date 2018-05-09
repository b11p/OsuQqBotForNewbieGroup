using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace OsuQqBot.Charts
{
    class Expression<T>
    {
        private readonly string _expression;
        private readonly IReadOnlyDictionary<string, (Action<Stack<double>> function, int priority)> _operatorsMap;
        private readonly IReadOnlyDictionary<string, Func<T, double>> _valuesMap;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="operatorsMap">不能包括字母、数字、下划线、括号以及空白字符。</param>
        /// <param name="valuesMap">字母或下划线开头，可以包括字母数字下划线。</param>
        /// <exception cref="FormatException"></exception>
        public Expression(string expression, IReadOnlyDictionary<string, (Action<Stack<double>> function, int priority)> operatorsMap, IReadOnlyDictionary<string, Func<T, double>> valuesMap)
        {
            _expression = expression;
            _operatorsMap = new Dictionary<string, (Action<Stack<double>> function, int priority)>(operatorsMap);
            _valuesMap = new Dictionary<string, Func<T, double>>(valuesMap);
            Init();
        }

        private void Init()
        {
            var processingData = new ProcessingData(_operatorsMap, _valuesMap);
            ProcessExpression(_expression, processingData);
        }

        /// <summary>
        /// 处理表达式。
        /// </summary>
        /// <exception cref="FormatException"></exception>
        private static void ProcessExpression(string expression, ProcessingData data)
        {
            int index = 0;
            ProcessExpression(expression, data, ref index);

            if (index < expression.Length) throw new FormatException("Analyzing error. Most possibly you lost an open parenthesis in the beginning.");

            while (data.Stack.TryPop(out string top))
            {
                data.Expression.AddLast(top);
            }
        }
        private static void ProcessExpression(string expression, ProcessingData data, ref int index)
        {
            while (index < expression.Length)
            {
                string next;

                // want: value or "("
                next = NextValueOrOpenParenthesis(expression, ref index);

                // 括号
                if (next == null) ProcessExpression(expression, data, ref index);
                // 数字入表达式
                else data.Expression.AddLast(next);

                // want: ")" or symbol or end
                next = NextSymbolOrEnd(expression, ref index);

                // 括号或结束
                if (next == null) return;
                // 入栈
                else data.SmartPush(next);

                // want: value or "("
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="data"></param>
        /// <param name="index"></param>
        /// <exception cref="FormatException">下一项不是数字、变量或左括号，或者读取已经结束。</exception>
        /// <returns>该数字或变量；如果为左括号，则为 <c>null</c>。</returns>
        private static string NextValueOrOpenParenthesis(string expression, ref int index)
        {
            int oldIndex = index;

            string next = ReadNext(expression, ref index);

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
        private static string NextSymbolOrEnd(string expression, ref int index)
        {
            int oldIndex = index;

            string next = ReadNext(expression, ref index);

            if (string.IsNullOrEmpty(next) || next == ")") return null;

            return !(IsValue(next) || next == "(") ? next : throw new FormatException($"Unexpected number or open parenthesis at {oldIndex}.");
        }

        /// <summary>
        /// 从指定索引开始读取下一项。
        /// </summary>
        /// <exception cref="FormatException"></exception>
        /// <returns>如果是末尾，则返回<c>null</c>；否则返回下一项。</returns>
        private static string ReadNext(string expression, ref int index)
        {
            index = SkipWhiteSpace(expression, index);

            if (index >= expression.Length) return null;

            var match = Regex.Match(expression.Substring(index),
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
            private readonly IReadOnlyDictionary<string, (Action<Stack<double>> function, int priority)> _operators;
            private readonly IReadOnlyDictionary<string, Func<T, double>> _variablesMap;
            private int _operatorsCount = 0;
            private int _variablesCount = 0;

            private readonly Stack<string> _stack = new Stack<string>();
            public LinkedList<string> Expression { get; } = new LinkedList<string>();

            public ProcessingData(IReadOnlyDictionary<string, (Action<Stack<double>> function, int priority)> operators, IReadOnlyDictionary<string, Func<T, double>> variablesMap)
            {
                _operators = operators;
                _variablesMap = variablesMap;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="symbol"></param>
            /// <exception cref="FormatException"><c>symbol</c> 不是合法的运算符。</exception>
            public void SmartPush(string symbol)
            {
                // TODO: 阻止操作符数量超过操作数的数量。
                if (!_operators.TryGetValue(symbol, out var oInfo)) throw new FormatException($"Invalid operator {symbol}.");
                int newPriority = oInfo.priority;
                while (_stack.TryPeek(out string top) && _operators[top].priority >= newPriority)
                {
                    _stack.Pop();
                    Expression.AddLast(top);
                }
                _stack.Push(symbol);
                _operatorsCount++;
            }

            public void FeedValue(string value)
            {
                if (!double.TryParse(value, out _) && !_variablesMap.ContainsKey(value)) throw new FormatException("Invalid value or varaiable name.");

                Expression.AddLast(value);
                _variablesCount++;
            }
        }
    }
}
