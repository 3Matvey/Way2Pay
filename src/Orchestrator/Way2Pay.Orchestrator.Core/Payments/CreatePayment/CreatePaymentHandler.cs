namespace Way2Pay.Orchestrator.Core.Payments.CreatePayment;

public sealed class CreatePaymentHandler(IPaymentWriter paymentWriter, TimeProvider timeProvider)
{
    public async Task<CreatePaymentResult> HandleAsync(
        CreatePaymentCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var payment = Payment.Create(
            command.ExternalPaymentId,
            command.Amount,
            command.CurrencyCode,
            command.CountryCode,
            command.Description,
            timeProvider.GetUtcNow().UtcDateTime);

        await paymentWriter.AddAsync(payment, cancellationToken);

        return new CreatePaymentResult(payment.Id, payment.Status);
    }
}
