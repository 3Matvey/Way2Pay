using Microsoft.AspNetCore.Mvc;
using Way2Pay.CardVault.Authentication;
using Way2Pay.CardVault.Errors;
using Way2Pay.Common.Results;

namespace Way2Pay.CardVault.Cards.RegisterCard;

internal static class RegisterCardEndpoint
{
    /// <summary>Maps card registration protected by a short-lived registration-session token.</summary>
    internal static RouteHandlerBuilder MapRegisterCard(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/cards", HandleAsync)
            .RequireAuthorization(CardRegistrationAuthentication.Policy)
            .WithName("RegisterCard")
            .WithGroupName("public-v1")
            .WithTags("Cards")
            .WithSummary("Registers a card using a registration-session token.")
            .Produces<RegisterCardResponse>(StatusCodes.Status201Created)
            .Produces<Error>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<Error>(StatusCodes.Status409Conflict)
            .Produces<Error>(StatusCodes.Status500InternalServerError);

    private static async Task<IResult> HandleAsync(
        RegisterCardRequest request, HttpContext httpContext,
        [FromServices] RegisterCardHandler handler, CancellationToken cancellationToken)
    {
        var context = httpContext.Features.Get<CardRegistrationContext>();
        if (context is null)
            return TypedResults.Forbid(authenticationSchemes: [CardRegistrationAuthentication.Scheme]);

        var result = await handler.ExecuteAsync(request, context, cancellationToken);
        if (!result.IsSuccess)
            return result.Error.ToHttpResult(CardRegistrationAuthentication.Scheme);

        return TypedResults.Created(uri: (string?)null, value: result.Value);
    }
}
