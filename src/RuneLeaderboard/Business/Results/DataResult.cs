using Api.Constants;

namespace Api.Business.Results;

public class DataResult<T> : Result where T : class
{
    public T? Data { get; set; }
    public DataResult(string? message = null, T? data = null, ResultStatus status = ResultStatus.Ok) : base(message, status)
    {
        Data = data;
    }

    public static new DataResult<T> InvalidRequest()
        => new(message: Messages.RequestInvalid, status: ResultStatus.RequestInvalid);

    public static new DataResult<T> Error()
    => new(message: Messages.UnexpectedError, status: ResultStatus.Error);
}
