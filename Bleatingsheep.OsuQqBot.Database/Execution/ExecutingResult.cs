using System;

namespace Bleatingsheep.OsuQqBot.Database.Execution
{
    public sealed class ExecutingResult<T>
    {
        private readonly T _result;

        internal ExecutingResult(Exception exception)
        {
            Success = false;
            Exception = exception;
        }

        internal ExecutingResult(T result)
        {
            Success = true;
            _result = result;
        }

        public bool Success { get; }

        public T Result => Success ? _result : throw new AggregateException(Exception);

        public Exception Exception { get; }

        public ExecutingResult<T> EnsureSuccess() => Success ? this : throw new AggregateException(Exception);
    }
}
