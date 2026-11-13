using Microsoft.AspNetCore.Mvc;
using Way2Pay.CardVault.Authentication;
using Way2Pay.CardVault.Errors;
using Way2Pay.Common.Results;

namespace Way2Pay.CardVault.CardRegistrationSessions.CreateCardRegistrationSession;

internal static class CreateCardRegistrationSessionEndpoint
{
    internal static RouteHandlerBuilder MapCreateCardRegistrationSession(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/card-registration-sessions", HandleAsync)
            .WithName("CreateCardRegistrationSession")
            .Produces<CreateCardRegistrationSessionResponse>(StatusCodes.Status201Created)
            .Produces<Error>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden);

    private static async Task<IResult> HandleAsync(
        CreateCardRegistrationSessionRequest request, HttpContext httpContext,
        [FromServices] CreateCardRegistrationSessionHandler handler, CancellationToken cancellationToken)
    {
        var merchant = GatewayMerchantContext.FromHeaders(httpContext.Request.Headers);
        if (merchant is null)
            return TypedResults.StatusCode(StatusCodes.Status403Forbidden);

        var result = await handler.ExecuteAsync(request, merchant.MerchantId, cancellationToken);
        if (!result.IsSuccess)
            return result.Error.ToHttpResult();

        httpContext.Response.Headers.CacheControl = "no-store";
        return TypedResults.Created(uri: (string?)null, value: result.Value);
    }
}
