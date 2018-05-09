using System;
using System.Collections.Generic;
using System.Text;

namespace OsuQqBot.Charts
{
    class Expression<T>
    {
        private readonly string _expression;
        private readonly IReadOnlyDictionary<string, (Action<Stack<double>> function, int priority)> _operators;
        private readonly IReadOnlyDictionary<string, Func<T, double>> _values;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="operators"></param>
        /// <param name="values"></param>
        /// <exception cref="FormatException"></exception>
        public Expression(string expression, IReadOnlyDictionary<string, (Action<Stack<double>> function, int priority)> operators, IReadOnlyDictionary<string, Func<T, double>> values)
        {

        }
    }
}
