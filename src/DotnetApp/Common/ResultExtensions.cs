using DotnetApp.Common.Errors;

namespace DotnetApp.Common;

/// <summary>
/// Maps a domain <see cref="Result"/> to an HTTP response. Centralizes the
/// ErrorType → status-code mapping so every endpoint is consistent.
/// </summary>
public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult> onSuccess) =>
        result.IsSuccess ? onSuccess(result.Value) : Problem(result.Error!);

    public static IResult ToHttpResult(this Result result, Func<IResult> onSuccess) =>
        result.IsSuccess ? onSuccess() : Problem(result.Error!);

    private static IResult Problem(Error error)
    {
        var status = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        // RFC 7807 ProblemDetails — the standard error shape for HTTP APIs.
        return Results.Problem(
            detail: error.Message,
            statusCode: status,
            title: error.Code);
    }
}
