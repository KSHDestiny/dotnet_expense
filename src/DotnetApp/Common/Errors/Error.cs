namespace DotnetApp.Common.Errors;

/// <summary>
/// Category of an expected failure. The API layer maps this to an HTTP status code,
/// so the service layer stays free of HTTP concerns.
/// </summary>
public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden
}

/// <summary>
/// An expected, non-exceptional failure. Carries a stable <paramref name="Code"/>
/// (machine-readable), a human <paramref name="Message"/>, and a <see cref="ErrorType"/>.
/// </summary>
public sealed record Error(string Code, string Message, ErrorType Type)
{
    public static Error Validation(string message, string code = "validation") =>
        new(code, message, ErrorType.Validation);

    public static Error NotFound(string message, string code = "not_found") =>
        new(code, message, ErrorType.NotFound);

    public static Error Conflict(string message, string code = "conflict") =>
        new(code, message, ErrorType.Conflict);

    public static Error Unauthorized(string message, string code = "unauthorized") =>
        new(code, message, ErrorType.Unauthorized);

    public static Error Forbidden(string message, string code = "forbidden") =>
        new(code, message, ErrorType.Forbidden);
}
