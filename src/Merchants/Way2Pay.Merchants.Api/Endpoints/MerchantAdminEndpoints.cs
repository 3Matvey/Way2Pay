using Microsoft.AspNetCore.Mvc;
using Way2Pay.Common.Results;
using Way2Pay.Merchants.Api.Errors;
using Way2Pay.Merchants.Application.ApiKeys.IssueApiKey;
using Way2Pay.Merchants.Application.Merchants.CreateMerchant;

namespace Way2Pay.Merchants.Api.Endpoints;

internal static class MerchantAdminEndpoints
{
    internal const string DocumentName = "admin-v1";

    internal static void MapMerchantAdminEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var merchants = endpoints.MapGroup("/admin/merchants")
            .WithGroupName(DocumentName)
            .WithTags("Merchant administration");
        
        merchants.MapPost("", CreateMerchantAsync)
            .WithSummary("Creates a merchant.")
            .Produces<CreateMerchantResponse>(StatusCodes.Status201Created)
            .Produces<Error>(StatusCodes.Status400BadRequest);

        merchants.MapPost("/{merchantId:guid}/api-keys", IssueApiKeyAsync)
            .WithSummary("Issues an API key. The full key is returned only at issuance.")
            .Produces<IssueApiKeyResponse>(StatusCodes.Status201Created)
            .Produces<Error>(StatusCodes.Status400BadRequest)
            .Produces<Error>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> CreateMerchantAsync(
        CreateMerchantRequest request, [FromServices] CreateMerchantUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(request, cancellationToken);
        return result.IsSuccess
            ? TypedResults.Created(uri: (string?)null, value: result.Value)
            : result.Error.ToHttpResult();
    }

    private static async Task<IResult> IssueApiKeyAsync(
        Guid merchantId, IssueApiKeyBody body, HttpContext httpContext,
        [FromServices] IssueApiKeyUseCase useCase, CancellationToken cancellationToken)
    {
        var request = new IssueApiKeyRequest(merchantId, body.Name, body.ExpiresAt);
        var result = await useCase.ExecuteAsync(request, cancellationToken);
        if (!result.IsSuccess)
            return result.Error.ToHttpResult();

        httpContext.Response.Headers.CacheControl = "no-store";
        return TypedResults.Created(uri: (string?)null, value: result.Value);
    }

    private sealed record IssueApiKeyBody(string Name, DateTimeOffset? ExpiresAt = null);
}
