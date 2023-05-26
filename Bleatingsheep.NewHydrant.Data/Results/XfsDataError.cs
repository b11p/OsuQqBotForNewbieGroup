using System;

namespace Bleatingsheep.NewHydrant.Data.Results;
public sealed class XfsDataError
{
    public XfsDataError(ErrorKind kind, Exception? exception = default)
    {
        Kind = kind;
        Exception = exception;
    }

    public ErrorKind Kind { get; }
    public Exception? Exception { get; }

    public enum ErrorKind
    {
        NoBinding,
        DatabaseError,
        DatabaseConcurrencyError,
        ExternalError,
    }
}