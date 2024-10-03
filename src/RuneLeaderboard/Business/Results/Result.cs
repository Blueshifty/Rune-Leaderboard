using Api.Constants;
using System.Text.Json.Serialization;

namespace Api.Business.Results;

public class Result
{
    public string? Message { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ResultStatus Status { get; set; }

    public Result(string? message = null, ResultStatus status = ResultStatus.Ok)
    {
        Message = message;
        Status = status;
    }

    public static Result Success() => new Result();

    public static Result InvalidRequest()
    => new(message: Messages.RequestInvalid, status: ResultStatus.RequestInvalid);

    public static Result Error()
    => new(message: Messages.UnexpectedError, status: ResultStatus.Error);
}


public enum ResultStatus
{
    Ok = 1,
    Error = 2,
    Forbidden = 3,
    RequestInvalid = 4,
    NotFound = 5
}