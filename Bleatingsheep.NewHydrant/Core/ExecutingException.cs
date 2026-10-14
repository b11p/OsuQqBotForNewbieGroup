using System;
using System.Linq;

namespace Bleatingsheep.NewHydrant.Core
{

    [Serializable]
    public class ExecutingException : Exception
    {
        public ExecutingException(string message) : base(message) { }
        public ExecutingException(string message, Exception inner) : base(message, inner) { }

        public static void Ensure(string onFalse, params bool[] success)
        {
            if (success.Contains(false)) throw new ExecutingException(onFalse);
        }

        public static void Ensure(bool success, string onFalse)
        {
            if (!success) throw new ExecutingException(onFalse);
        }

        public static void Ensure(Func<bool> test, string onFalse) => Ensure(test(), onFalse);

        public static void Cannot(bool isFail, string onFail) => Ensure(!isFail, onFail);
    }
}
