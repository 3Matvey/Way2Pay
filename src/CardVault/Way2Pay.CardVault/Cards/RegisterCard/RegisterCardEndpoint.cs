using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Way2Pay.CardVault.Authentication;
using Way2Pay.Common.Results;

namespace Way2Pay.CardVault.Cards.RegisterCard;

internal static class RegisterCardEndpoint
{
    /// <summary>Maps card registration; authentication and verified registration-scope enrichment must be configured first.</summary>
    internal static RouteHandlerBuilder MapRegisterCard(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/cards", HandleAsync)
            .RequireAuthorization()
            .WithName("RegisterCard")
            .WithGroupName("public-v1")
            .WithTags("Cards");

    private static async Task<Results<Created<RegisterCardResponse>, BadRequest<Error>, ForbidHttpResult>> HandleAsync(
        RegisterCardRequest request, HttpContext httpContext,
        [FromServices] RegisterCardHandler handler, CancellationToken cancellationToken)
    {
        var context = httpContext.Features.Get<CardRegistrationContext>();
        if (context is null)
            return TypedResults.Forbid();

        var result = await handler.ExecuteAsync(request, context, cancellationToken);
        if (!result.IsSuccess)
            return TypedResults.BadRequest(result.Error);

        return TypedResults.Created(uri: (string?)null, value: result.Value);
    }
}
