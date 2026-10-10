using Way2Pay.Payments.Domain.Payments;

namespace Way2Pay.Payments.Application.Payments;

public interface IPaymentRepository
{
    /// <summary>
    /// Loads a payment owned by the merchant, including operation routes with steps ordered by Position
    /// and attempts ordered by Number. Concurrent reads must not observe uncommitted attempt claims.
    /// Saving must detect concurrent aggregate changes, including child additions, and reject them
    /// atomically with PaymentConcurrencyException. Missing and foreign payments both return null.
    /// </summary>
    Task<Payment?> GetByIdAsync(
        Guid merchantId, Guid paymentId, CancellationToken cancellationToken = default);

    /// <summary>Stages the payment without saving; persistence is performed by the unit of work.</summary>
    void Add(Payment payment);
}
