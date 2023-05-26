using System;
using System.Diagnostics.CodeAnalysis;

namespace Bleatingsheep.NewHydrant.Data.Results;

public static class XfsDataResultExtensions
{
    public static bool TryGetResult<TResult, TError>(this IXfsDataResult<TResult, TError> result, [MaybeNullWhen(false)] out TResult okResult)
    {
        if (result.IsOk)
        {
            okResult = result.OkResult;
            return true;
        }
        okResult = default;
        return false;
    }

    public static bool TryGetError<TResult, TError>(this IXfsDataResult<TResult, TError> result, [MaybeNullWhen(false)] out TError error)
    {
        if (result.IsOk)
        {
            error = default;
            return false;
        }
        error = result.Error;
        return true;
    }

    public static void Match<TResult, TError>(this IXfsDataResult<TResult, TError> result, Action<TResult> whenOk, Action<TError> whenError)
    {
        if (result.IsOk)
        {
            whenOk?.Invoke(result.OkResult);
        }
        else
        {
            whenError?.Invoke(result.Error);
        }
    }

    public static T Match<TResult, TError, T>(this IXfsDataResult<TResult, TError> result, Func<TResult, T> whenOk, Func<TError, T> whenError)
    {
        if (result.IsOk)
        {
            ArgumentNullException.ThrowIfNull(whenOk);
            return whenOk.Invoke(result.OkResult);
        }
        else
        {
            ArgumentNullException.ThrowIfNull(whenError);
            return whenError.Invoke(result.Error);
        }
    }
}
