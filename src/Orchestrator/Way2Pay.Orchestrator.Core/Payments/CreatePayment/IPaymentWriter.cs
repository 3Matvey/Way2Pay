namespace Way2Pay.Orchestrator.Core.Payments.CreatePayment;

public interface IPaymentWriter
{
    Task AddAsync(Payment payment, CancellationToken cancellationToken);
}
