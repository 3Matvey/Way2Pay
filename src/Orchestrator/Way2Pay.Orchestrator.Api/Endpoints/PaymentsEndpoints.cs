using Microsoft.AspNetCore.Mvc;

namespace Way2Pay.Orchestrator.Api.Endpoints;

public static class PaymentsEndpoints
{
    public static IEndpointRouteBuilder MapPaymentsEndpoints(this IEndpointRouteBuilder app)
    {
        var payments = app.MapGroup("/payments").WithTags("Payments");

        payments.MapPost("/", CreatePayment)
            .WithName("CreatePayment")
            .WithSummary("Create a payment")
            .ProducesProblem(StatusCodes.Status501NotImplemented);

        return app;
    }

    private static IResult CreatePayment(
        CreatePaymentRequest request,
        [FromHeader(Name = "Idempotency-Key")] string idempotencyKey)
    {
        // The HTTP contract is defined first; the Core use case and persistence come next.
        return Results.Problem(
            detail: "Payment creation is not implemented yet.",
            statusCode: StatusCodes.Status501NotImplemented);
    }
}

public sealed record CreatePaymentRequest(
    string? ExternalPaymentId,
    decimal Amount,
    string CurrencyCode,
    string? CountryCode,
    string? Description);
