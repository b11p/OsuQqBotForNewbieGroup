using System;

namespace Bleatingsheep.OsuQqBot.Database.Execution
{
    public static class ExecutingResultExtensions
    {
        public static bool TryGetResult<T>(this IExecutingResult<T> executingResult, out T result)
        {
            if (executingResult == null)
            {
                throw new ArgumentNullException(nameof(executingResult));
            }

            result = executingResult.Success ? executingResult.Result : default(T);
            return executingResult.Success;
        }
    }
}
