using Way2Pay.Payments.Domain.Common;

namespace Way2Pay.Payments.Domain.Payments.Attempts;

/// <summary>A provider call belonging to a single payment operation.</summary>
public sealed class PaymentAttempt : AuditableEntity
{
    public Guid PaymentOperationId { get; private set; }
    public Guid ProviderAccountId { get; private set; }
    public int Number { get; private set; }
    public PaymentAttemptStatus Status { get; private set; }
    /// <summary>The provider transaction identifier, which may be known before the final outcome.</summary>
    /// <remarks>Recorded on success or Unknown and never replaced with a different ID within the attempt.</remarks>
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
        RecordProviderTransactionId(providerTransactionId);
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

    /// <summary>Marks a processing attempt as Unknown and records the known transaction ID.</summary>
    /// <param name="providerTransactionId">An optional transaction ID; null means the provider has not supplied one.</param>
    /// <remarks>CompletedAt remains unset because the monetary outcome has not been established.</remarks>
    internal void MarkUnknown(string? providerTransactionId)
    {
        if (Status != PaymentAttemptStatus.Processing)
            throw new InvalidOperationException("Only a processing attempt can become unknown.");

        if (providerTransactionId is not null)
            RecordProviderTransactionId(providerTransactionId);
        Status = PaymentAttemptStatus.Unknown;
    }

    /// <summary>Binds the transaction ID to the attempt so a later result cannot substitute a different transaction.</summary>
    private void RecordProviderTransactionId(string transactionId)
    {
        Guard.Required(transactionId);
        if (ProviderTransactionId is not null && ProviderTransactionId != transactionId)
            throw new InvalidOperationException("The provider transaction identifier cannot change within an attempt.");
        ProviderTransactionId = transactionId;
    }

    private void EnsureUnresolved()
    {
        if (Status is not (PaymentAttemptStatus.Processing or PaymentAttemptStatus.Unknown))
            throw new InvalidOperationException("The attempt is already complete.");
    }

}
