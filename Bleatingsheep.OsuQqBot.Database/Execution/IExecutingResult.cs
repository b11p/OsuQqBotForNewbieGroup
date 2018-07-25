using System;

namespace Bleatingsheep.OsuQqBot.Database.Execution
{
    public interface IExecutingResult
    {
        Exception Exception { get; }
        bool Success { get; }
    }

    public interface IExecutingResult<out T> : IExecutingResult
    {
        T Result { get; }

        IExecutingResult<T> EnsureSuccess();
        IExecutingResult<T> EnsureSuccess(string message);
        IExecutingResult<TResult> TryGet<TResult>(Func<T, TResult> func);
        bool TryGet<TResult>(Func<T, TResult> func, out TResult result);
    }
}