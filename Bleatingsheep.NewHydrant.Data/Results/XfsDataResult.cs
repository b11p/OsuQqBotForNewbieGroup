using System;

namespace Bleatingsheep.NewHydrant.Data.Results;
public static class XfsDataResult
{
    public static XfsDataResult<TResult, TError> Ok<TResult, TError>(TResult result)
    {
        return new XfsDataResult<TResult, TError>(result);
    }

    public static XfsDataResult<TResult, XfsDataError> Ok<TResult>(TResult result)
    {
        return new XfsDataResult<TResult, XfsDataError>(result);
    }

    public static XfsDataResult<TResult, TError> Error<TResult, TError>(TError error)
    {
        return new XfsDataResult<TResult, TError>(error);
    }

    public static XfsDataResult<int, TError> Error<TError>(TError error)
    {
        return new XfsDataResult<int, TError>(error);
    }
}

public readonly struct XfsDataResult<TResult, TError> : IXfsDataResult<TResult, TError>
{
    private readonly bool _isOk;
    private readonly TResult _result;
    private readonly TError _error;

    public XfsDataResult(TResult result)
    {
        _result = result;
        _isOk = true;
        _error = default!;
    }

    public XfsDataResult(TError error)
    {
        _error = error;
        _isOk = false;
        _result = default!;
    }

    bool IXfsDataResult<TResult, TError>.IsOk => _isOk;

    TResult IXfsDataResult<TResult, TError>.OkResult
    {
        get
        {
            return _isOk ? _result : throw new InvalidOperationException("The result is not of success.");
        }
    }

    TError IXfsDataResult<TResult, TError>.Error
    {
        get
        {
            return _isOk
                ? throw new InvalidOperationException("The result is of success")
                : _error;
        }
    }
}