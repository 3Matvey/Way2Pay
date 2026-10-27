using Way2Pay.Common.Results;

namespace Way2Pay.CardVault.Errors;

/// <summary>Maps application errors to HTTP responses without choosing a service-specific authentication scheme.</summary>
internal static class ErrorHttpExtensions
{
    /// <remarks>Authentication failures invoke the selected scheme so its challenge headers are preserved.</remarks>
    internal static IResult ToHttpResult(this Error error, string? authenticationScheme = null) => error.ErrorType switch
    {
        ErrorType.BadRequest => TypedResults.BadRequest(error),
        ErrorType.NotFound => TypedResults.NotFound(error),
        ErrorType.Conflict => TypedResults.Conflict(error),
        ErrorType.AccessUnauthorized => TypedResults.Challenge(
            authenticationSchemes: authenticationScheme is null ? null : [authenticationScheme]),
        ErrorType.AccessForbidden => TypedResults.Forbid(
            authenticationSchemes: authenticationScheme is null ? null : [authenticationScheme]),
        _ => TypedResults.Json(error, statusCode: StatusCodes.Status500InternalServerError)
    };
}
