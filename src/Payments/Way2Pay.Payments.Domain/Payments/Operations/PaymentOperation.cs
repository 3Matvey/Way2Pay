using Way2Pay.Payments.Domain.Common;
using Way2Pay.Payments.Domain.Payments.Attempts;
using Way2Pay.Payments.Domain.ValueObjects;

namespace Way2Pay.Payments.Domain.Payments.Operations;

/// <summary>One monetary action, with possible retries after confirmed failures.</summary>
public sealed class PaymentOperation : AuditableEntity
{
    private readonly List<PaymentAttempt> _attempts = [];

    public Guid PaymentId { get; private set; }
    public PaymentOperationType Type { get; private set; }
    public Money Amount { get; }
    public PaymentOperationStatus Status { get; private set; }
    public string? OriginalProviderTransactionId { get; private set; }
    public PaymentOperationRoute? Route { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public IReadOnlyList<PaymentAttempt> Attempts => _attempts.AsReadOnly();

    internal PaymentOperation(
        Guid paymentId, PaymentOperationType type, Money amount,
        string? originalProviderTransactionId)
    {
        ArgumentNullException.ThrowIfNull(amount);
        Guard.Positive(amount.Amount);
        PaymentId = Guard.RequiredId(paymentId);
        Type = type;
        Amount = amount;
        OriginalProviderTransactionId = originalProviderTransactionId;
        Status = PaymentOperationStatus.Pending;
    }

    internal void SetRoute(PaymentOperationRoute route)
    {
        if (Status != PaymentOperationStatus.Pending || Route is not null || _attempts.Count != 0)
            throw new InvalidOperationException("The route can only be assigned once before the first attempt.");
        Route = route;
    }

    internal PaymentAttempt StartAttempt(Guid providerAccountId)
    {
        EnsurePendingOrProcessing();
        EnsureAllAttemptsFailed();
        var step = Route?.Steps.SingleOrDefault(candidate => candidate.ProviderAccountId == providerAccountId)
            ?? throw new InvalidOperationException("The provider account must belong to the saved operation route.");
        var attempt = new PaymentAttempt(Id, step.Id, providerAccountId, _attempts.Count + 1);
        _attempts.Add(attempt);
        Status = PaymentOperationStatus.Processing;
        return attempt;
    }

    internal PaymentAttempt GetCurrentAttempt(Guid attemptId)
    {
        EnsureProcessingOrUnknown();
        var attempt = _attempts.LastOrDefault();
        if (attempt is null || attempt.Id != attemptId)
            throw new InvalidOperationException("Only the current attempt can change the operation result.");

        return attempt;
    }

    internal void Succeed(DateTimeOffset now)
    {
        EnsureProcessingOrUnknown();
        if (_attempts.LastOrDefault()?.Status != PaymentAttemptStatus.Succeeded)
            throw new InvalidOperationException("The current attempt must have succeeded.");

        Status = PaymentOperationStatus.Succeeded;
        CompletedAt = now;
    }

    internal void MarkUnknown()
    {
        if (Status is not (PaymentOperationStatus.Processing or PaymentOperationStatus.Unknown)
            || _attempts.LastOrDefault()?.Status != PaymentAttemptStatus.Unknown)
            throw new InvalidOperationException("An unresolved operation requires an unknown current attempt.");

        Status = PaymentOperationStatus.Unknown;
    }

    internal void ResumeAfterConfirmedFailure()
    {
        if (Status is not (PaymentOperationStatus.Processing or PaymentOperationStatus.Unknown)
            || _attempts.Count == 0 || _attempts.Any(a => a.Status != PaymentAttemptStatus.Failed))
            throw new InvalidOperationException("Resuming requires a confirmed attempt failure.");

        Status = PaymentOperationStatus.Processing;
    }

    internal void Fail(DateTimeOffset now)
    {
        EnsurePendingOrProcessing();
        EnsureAllAttemptsFailed();
        Status = PaymentOperationStatus.Failed;
        CompletedAt = now;
    }

    private void EnsureProcessingOrUnknown()
    {
        if (Status is not (PaymentOperationStatus.Processing or PaymentOperationStatus.Unknown))
            throw new InvalidOperationException("The operation must be processing or unknown.");
    }

    private void EnsurePendingOrProcessing()
    {
        if (Status is not (PaymentOperationStatus.Pending or PaymentOperationStatus.Processing))
            throw new InvalidOperationException("The operation must be pending or processing.");
    }

    private void EnsureAllAttemptsFailed()
    {
        if (_attempts.Any(a => a.Status != PaymentAttemptStatus.Failed))
            throw new InvalidOperationException("All previous attempts must have failed definitively.");
    }
}
