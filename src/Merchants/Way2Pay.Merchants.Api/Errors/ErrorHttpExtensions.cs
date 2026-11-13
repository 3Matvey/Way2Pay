using Way2Pay.Common.Results;

namespace Way2Pay.Merchants.Api.Errors;

internal static class ErrorHttpExtensions
{
    internal static IResult ToHttpResult(this Error error) => error.ErrorType switch
    {
        ErrorType.BadRequest => TypedResults.BadRequest(error),
        ErrorType.NotFound => TypedResults.NotFound(error),
        ErrorType.Conflict => TypedResults.Conflict(error),
        ErrorType.AccessUnauthorized => TypedResults.Unauthorized(),
        ErrorType.AccessForbidden => TypedResults.StatusCode(StatusCodes.Status403Forbidden),
        _ => TypedResults.Problem(statusCode: StatusCodes.Status500InternalServerError)
    };
}
