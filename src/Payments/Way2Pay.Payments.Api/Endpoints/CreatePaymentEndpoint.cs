using Microsoft.AspNetCore.Mvc;
using Way2Pay.Common.Results;
using Way2Pay.Payments.Api.Authentication;
using Way2Pay.Payments.Application.Payments;
using Way2Pay.Payments.Application.Payments.CreatePayment;
using Way2Pay.Payments.Domain.ValueObjects;

namespace Way2Pay.Payments.Api.Endpoints;

internal static class CreatePaymentEndpoint
{
    private const string IdempotencyHeader = "Idempotency-Key";

    internal static RouteHandlerBuilder MapCreatePayment(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/payments", HandleAsync)
            .WithName("CreatePayment")
            .WithGroupName("public-v1")
            .WithTags("Payments")
            .WithSummary("Creates a payment or returns its original creation result for the same idempotency key.")
            .Produces<CreatePaymentResponse>(StatusCodes.Status201Created)
            .Produces<Error>(StatusCodes.Status400BadRequest)
            .Produces<Error>(StatusCodes.Status403Forbidden)
            .Produces<Error>(StatusCodes.Status409Conflict);

    private static async Task<IResult> HandleAsync(
        CreatePaymentBody body, [FromHeader(Name = IdempotencyHeader)] string? idempotencyKey,
        HttpContext context, [FromServices] GatewayMerchantAccess merchantAccess,
        [FromServices] CreatePaymentUseCase useCase, CancellationToken cancellationToken)
    {
        if (merchantAccess.MerchantId is not { } merchantId)
            return ToHttpResult(PaymentErrors.MerchantUnavailable());
        if (idempotencyKey is null || context.Request.Headers[IdempotencyHeader].Count != 1)
            return TypedResults.BadRequest(PaymentErrors.InvalidCreationIdempotencyKey());
        if (body.CardTokenId == Guid.Empty)
            return TypedResults.BadRequest(PaymentErrors.InvalidPaymentMethod());

        var request = new CreatePaymentRequest(merchantId, body.Amount, body.Currency,
            idempotencyKey, PaymentMethod.FromVaultCard(body.CardTokenId));
        var result = await useCase.ExecuteAsync(request, cancellationToken);
        return result.IsSuccess ? TypedResults.Created(uri: (string?)null, value: result.Value) : ToHttpResult(result.Error);
    }

    private static IResult ToHttpResult(Error error) => error.ErrorType switch
    {
        ErrorType.BadRequest => TypedResults.BadRequest(error),
        ErrorType.AccessForbidden => TypedResults.Json(error, statusCode: StatusCodes.Status403Forbidden),
        ErrorType.Conflict => TypedResults.Conflict(error),
        _ => TypedResults.Problem(statusCode: StatusCodes.Status500InternalServerError)
    };

    private sealed record CreatePaymentBody(decimal Amount, string Currency, Guid CardTokenId);
}
