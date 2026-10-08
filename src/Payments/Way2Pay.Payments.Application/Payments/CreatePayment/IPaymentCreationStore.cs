namespace Way2Pay.Payments.Application.Payments.CreatePayment;

/// <summary>Stores creation results atomically with payments in the same unit of work.</summary>
public interface IPaymentCreationStore
{
    /// <summary>Reads a committed result. Keys are compared exactly within each merchant.</summary>
    Task<PaymentCreationRecord?> FindAsync(
        Guid merchantId, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stages a record without saving it. The merchant/key pair must be unique.
    /// A competing committed creation must cause PaymentCreationConflictException on SaveChangesAsync,
    /// with neither the new payment nor its creation record committed.
    /// </summary>
    void Add(PaymentCreationRecord record);
}
