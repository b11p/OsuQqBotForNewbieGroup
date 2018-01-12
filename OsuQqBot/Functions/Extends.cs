using System;
using System.Collections.Generic;
using System.Text;

namespace OsuQqBot.Functions
{
    static class Extends
    {
        public static bool HasCQFunction(this string message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            return cQFunctionRegex.IsMatch(message);
        }

        private const string cQFunctionPatten = @"\[CQ:.+\]";
        private static readonly System.Text.RegularExpressions.Regex cQFunctionRegex = new System.Text.RegularExpressions.Regex(cQFunctionPatten);

    }
}
