using Way2Pay.Payments.Domain.Common;

namespace Way2Pay.Payments.Domain.Payments.Attempts;

/// <summary>A provider call belonging to a single payment operation.</summary>
public sealed class PaymentAttempt : AuditableEntity
{
    public Guid PaymentOperationId { get; private set; }
    /// <summary>The saved route step used for this provider call.</summary>
    public Guid RouteStepId { get; private set; }
    public Guid ProviderAccountId { get; private set; }
    public int Number { get; private set; }
    public PaymentAttemptStatus Status { get; private set; }
    /// <summary>The provider transaction identifier, which may be known before the final outcome.</summary>
    /// <remarks>Recorded on success, confirmed failure or Unknown and never replaced with a different ID within the attempt.</remarks>
    public string? ProviderTransactionId { get; private set; }
    public string? FailureCode { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    internal PaymentAttempt(Guid operationId, Guid routeStepId, Guid providerAccountId, int number)
    {
        if (number <= 0)
            throw new ArgumentOutOfRangeException(nameof(number), "The attempt number must be positive.");

        PaymentOperationId = Guard.RequiredId(operationId);
        RouteStepId = Guard.RequiredId(routeStepId);
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

    /// <summary>Records a confirmed failure without monetary effect and retains any supplied transaction ID.</summary>
    /// <remarks>Transport errors alone must be recorded as Unknown. An omitted ID preserves any previously recorded ID.</remarks>
    internal void Fail(string failureCode, DateTimeOffset now, string? providerTransactionId = null)
    {
        EnsureUnresolved();
        Guard.Required(failureCode);
        if (providerTransactionId is not null)
            RecordProviderTransactionId(providerTransactionId);
        FailureCode = failureCode;
        Status = PaymentAttemptStatus.Failed;
        CompletedAt = now;
    }

    /// <summary>Marks an unresolved attempt as Unknown or adds a newly learned transaction ID to an Unknown attempt.</summary>
    /// <param name="providerTransactionId">An optional transaction ID; null means the provider has not supplied one.</param>
    /// <remarks>CompletedAt remains unset because the monetary outcome has not been established.</remarks>
    internal void MarkUnknown(string? providerTransactionId)
    {
        EnsureUnresolved();
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
