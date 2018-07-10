using System;

namespace Bleatingsheep.OsuQqBot.Database.Execution
{
    public interface IExecutingResult<out T>
    {
        Exception Exception { get; }
        T Result { get; }
        bool Success { get; }

        IExecutingResult<T> EnsureSuccess();
        IExecutingResult<TResult> TryGet<TResult>(Func<T, TResult> func);
        bool TryGet<TResult>(Func<T, TResult> func, out TResult result);
    }
}