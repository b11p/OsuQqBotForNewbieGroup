using System;

namespace Bleatingsheep.OsuQqBot.Database.Execution
{
    public sealed class ExecutingResult<T> : IExecutingResult<T>
    {
        private readonly T _result;

        internal ExecutingResult(Exception exception)
        {
            Success = false;
            Exception = exception ?? throw new ArgumentNullException(nameof(exception));
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

        IExecutingResult<T> IExecutingResult<T>.EnsureSuccess() => EnsureSuccess();

        public bool TryGetResult(out T result)
        {
            result = Success ? _result : default(T);
            return Success;
        }

        public bool TryGet<TResult>(Func<T, TResult> func, out TResult result)
        {
            result = Success ? func(Result) : default(TResult);
            return Success;
        }

        public ExecutingResult<TResult> TryGet<TResult>(Func<T, TResult> func)
            => Success
                ? new ExecutingResult<TResult>(func(Result))
                : new ExecutingResult<TResult>(Exception);

        IExecutingResult<TResult> IExecutingResult<T>.TryGet<TResult>(Func<T, TResult> func) => TryGet(func);
    }
}
