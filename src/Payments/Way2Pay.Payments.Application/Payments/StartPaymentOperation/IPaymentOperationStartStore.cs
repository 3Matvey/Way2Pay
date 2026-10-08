namespace Way2Pay.Payments.Application.Payments.StartPaymentOperation;

/// <summary>Stores operation-start results atomically with the payment aggregate.</summary>
public interface IPaymentOperationStartStore
{
    /// <summary>Reads the latest committed result. Keys are compared exactly within each merchant.</summary>
    Task<PaymentOperationStartRecord?> FindAsync(
        Guid merchantId, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stages a record without saving. The merchant/key pair is unique within this scenario.
    /// A competing committed record must cause PaymentOperationStartConflictException on SaveChangesAsync,
    /// with no changes from the losing command committed.
    /// </summary>
    void Add(PaymentOperationStartRecord record);
}
