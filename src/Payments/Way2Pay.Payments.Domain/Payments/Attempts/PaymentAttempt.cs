using Way2Pay.Payments.Domain.Common;

namespace Way2Pay.Payments.Domain.Payments.Attempts;

/// <summary>A provider call belonging to a single payment operation.</summary>
public sealed class PaymentAttempt : AuditableEntity
{
    public Guid PaymentOperationId { get; private set; }
    public Guid ProviderAccountId { get; private set; }
    public int Number { get; private set; }
    public PaymentAttemptStatus Status { get; private set; }
    public string? ProviderTransactionId { get; private set; }
    public string? FailureCode { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    internal PaymentAttempt(Guid operationId, Guid providerAccountId, int number)
    {
        if (number <= 0)
            throw new ArgumentOutOfRangeException(nameof(number), "The attempt number must be positive.");

        PaymentOperationId = Guard.RequiredId(operationId);
        ProviderAccountId = Guard.RequiredId(providerAccountId);
        Number = number;
        Status = PaymentAttemptStatus.Processing;
    }

    internal void Succeed(string providerTransactionId, DateTimeOffset now)
    {
        EnsureUnresolved();
        ProviderTransactionId = Guard.Required(providerTransactionId);
        Status = PaymentAttemptStatus.Succeeded;
        CompletedAt = now;
    }

    // Failure means definitive evidence that the operation was not performed.
    // Transport errors alone must be recorded as Unknown instead.
    internal void Fail(string failureCode, DateTimeOffset now)
    {
        EnsureUnresolved();
        FailureCode = Guard.Required(failureCode);
        Status = PaymentAttemptStatus.Failed;
        CompletedAt = now;
    }

    internal void MarkUnknown()
    {
        if (Status != PaymentAttemptStatus.Processing)
            throw new InvalidOperationException("Only a processing attempt can become unknown.");

        Status = PaymentAttemptStatus.Unknown;
    }

    private void EnsureUnresolved()
    {
        if (Status is not (PaymentAttemptStatus.Processing or PaymentAttemptStatus.Unknown))
            throw new InvalidOperationException("The attempt is already complete.");
    }

}
