using Microsoft.AspNetCore.Diagnostics;
using Way2Pay.Common.Results;

namespace Way2Pay.CardVault.Errors;

/// <summary>Returns safe error responses for exceptions escaping the request pipeline.</summary>
internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // Exception messages and data may contain credentials; record only type and request correlation.
        logger.Log(exception is BadHttpRequestException ? LogLevel.Warning : LogLevel.Error,
            "Request failed with {ExceptionType}. Trace identifier: {TraceId}.",
            exception.GetType().FullName, httpContext.TraceIdentifier);
        await CreateResponse(exception).ExecuteAsync(httpContext);
        return true;
    }

    private static IResult CreateResponse(Exception exception) => exception switch
    {
        BadHttpRequestException requestError => TypedResults.Json(
            Error.BadRequest("CardVault.InvalidRequest", "The HTTP request is invalid."),
            statusCode: requestError.StatusCode),
        _ => Error.Failure("CardVault.UnexpectedError", "An unexpected error occurred.").ToHttpResult()
    };
}
