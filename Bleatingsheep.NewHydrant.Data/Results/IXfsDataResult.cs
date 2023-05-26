namespace Bleatingsheep.NewHydrant.Data.Results;
public interface IXfsDataResult<out TResult, out TError>
{
    bool IsOk { get; }
    TResult OkResult { get; }
    TError Error { get; }
}
