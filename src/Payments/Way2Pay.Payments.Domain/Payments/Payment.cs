using Way2Pay.Payments.Domain.Common;
using Way2Pay.Payments.Domain.Payments.Attempts;
using Way2Pay.Payments.Domain.Payments.Operations;
using Way2Pay.Payments.Domain.ValueObjects;

namespace Way2Pay.Payments.Domain.Payments;

/// <summary>Aggregate root controlling monetary operations and their provider attempts.</summary>
public sealed class Payment : AuditableEntity
{
    private readonly List<PaymentOperation> _operations = [];

    public Guid MerchantId { get; private set; }
    public Money Amount { get; }
    public CurrencyCode Currency => Amount.Currency;
    public PaymentStatus Status { get; private set; }
    public PaymentBalance Balance { get; }
    public IReadOnlyList<PaymentOperation> Operations => _operations.AsReadOnly();

    public Payment(Guid merchantId, Money amount)
    {
        ArgumentNullException.ThrowIfNull(amount);
        Guard.Positive(amount.Amount);
        MerchantId = Guard.RequiredId(merchantId);
        Amount = amount;
        Balance = new PaymentBalance(Currency);
        Status = PaymentStatus.Created;
    }

    public PaymentOperation StartOperation(PaymentOperationType type, Money amount)
    {
        EnsureNoActiveOperation();
        EnsureOperationAllowed(type, amount);
        var operation = new PaymentOperation(Id, type, amount, GetOriginalAttempt(type)?.ProviderTransactionId);
        _operations.Add(operation);
        return operation;
    }

    public PaymentAttempt StartAttempt(Guid operationId, Guid providerAccountId)
    {
        var operation = GetActiveOperation(operationId);
        if (operation.Type is not (PaymentOperationType.Authorize or PaymentOperationType.Charge)
            && GetOriginalAttempt(operation.Type)?.ProviderAccountId != providerAccountId)
            throw new InvalidOperationException("Follow-up operations must use the original provider account.");

        return operation.StartAttempt(providerAccountId);
    }

    public void RecordAttemptSuccess(
        Guid operationId, Guid attemptId, string providerTransactionId, DateTimeOffset now)
    {
        var operation = GetActiveOperation(operationId);
        var attempt = operation.GetCurrentAttempt(attemptId);
        EnsureOperationAllowed(operation.Type, operation.Amount);
        attempt.Succeed(providerTransactionId, now);
        ApplySuccessfulOperation(operation);
        operation.Succeed(now);
    }

    /// <summary>Records a verified failure with no monetary effect. A timeout is not such evidence.</summary>
    public void RecordAttemptFailure(Guid operationId, Guid attemptId, string failureCode, DateTimeOffset now)
    {
        var operation = GetActiveOperation(operationId);
        operation.GetCurrentAttempt(attemptId).Fail(failureCode, now);
        operation.ResumeAfterConfirmedFailure();
    }

    public void MarkAttemptUnknown(Guid operationId, Guid attemptId)
    {
        var operation = GetActiveOperation(operationId);
        operation.GetCurrentAttempt(attemptId).MarkUnknown();
        operation.MarkUnknown();
    }

    /// <summary>Finishes an operation when no further safe attempts should be made.</summary>
    public void FailOperation(Guid operationId, DateTimeOffset now)
    {
        var operation = GetActiveOperation(operationId);
        operation.Fail(now);
        if (operation.Type is PaymentOperationType.Authorize or PaymentOperationType.Charge)
            Status = PaymentStatus.Failed;
    }

    private void EnsureOperationAllowed(PaymentOperationType type, Money amount)
    {
        ArgumentNullException.ThrowIfNull(amount);
        Guard.Positive(amount.Amount);
        if (amount.Currency != Currency)
            throw new InvalidOperationException("The operation must use the payment currency.");
        if (!CanStartOperation(type, amount))
            throw new InvalidOperationException("The operation or amount is not allowed for the current payment state.");
    }

    private bool CanStartOperation(PaymentOperationType type, Money amount) 
        => type switch
        {
            PaymentOperationType.Authorize or PaymentOperationType.Charge =>
                Status == PaymentStatus.Created && amount == Amount,
            PaymentOperationType.Capture or PaymentOperationType.Void =>
                Status == PaymentStatus.Authorized && GetOriginalAttempt(type) is not null && amount == Balance.Authorized,
            PaymentOperationType.Refund =>
                (Status is PaymentStatus.Captured or PaymentStatus.PartiallyRefunded)
                && GetOriginalAttempt(type) is not null && Balance.CanRefund(amount),
            _ => false
        };

    private PaymentAttempt? GetOriginalAttempt(PaymentOperationType type)
    {
        var operation = type switch
        {
            PaymentOperationType.Capture or PaymentOperationType.Void => _operations.SingleOrDefault(o => 
                o.Type == PaymentOperationType.Authorize && o.Status == PaymentOperationStatus.Succeeded),

            PaymentOperationType.Refund => _operations.SingleOrDefault(o =>
                (o.Type is PaymentOperationType.Charge or PaymentOperationType.Capture) && o.Status == PaymentOperationStatus.Succeeded),

            _ => null
        };
        return operation?.Attempts.Single(a => a.Status == PaymentAttemptStatus.Succeeded);
    }

    private void ApplySuccessfulOperation(PaymentOperation operation)
    {
        switch (operation.Type)
        {
            case PaymentOperationType.Authorize:
                ApplyAuthorization(operation);
                break;
            case PaymentOperationType.Charge:
                ApplyCharge(operation);
                break;
            case PaymentOperationType.Capture:
                ApplyCapture(operation);
                break;
            case PaymentOperationType.Void:
                ApplyVoid();
                break;
            case PaymentOperationType.Refund:
                ApplyRefund(operation);
                break;
            default:
                throw new InvalidOperationException("The operation type is not supported.");
        }
    }

    private void ApplyAuthorization(PaymentOperation operation)
    {
        Balance.Authorize(operation.Amount);
        Status = PaymentStatus.Authorized;
    }

    private void ApplyCharge(PaymentOperation operation)
    {
        Balance.Charge(operation.Amount);
        Status = PaymentStatus.Captured;
    }

    private void ApplyCapture(PaymentOperation operation)
    {
        Balance.Capture(operation.Amount);
        Status = PaymentStatus.Captured;
    }

    private void ApplyVoid()
    {
        Balance.Void();
        Status = PaymentStatus.Canceled;
    }

    private void ApplyRefund(PaymentOperation operation)
    {
        Balance.Refund(operation.Amount);
        Status = Balance.Refunded == Balance.Captured ? PaymentStatus.Refunded : PaymentStatus.PartiallyRefunded;
    }

    private PaymentOperation GetActiveOperation(Guid operationId)
    {
        var operation = _operations.SingleOrDefault(o => o.Id == operationId)
            ?? throw new InvalidOperationException("The operation does not belong to this payment.");
        if (operation.Status is PaymentOperationStatus.Succeeded or PaymentOperationStatus.Failed)
            throw new InvalidOperationException("The operation is already complete.");

        return operation;
    }

    private void EnsureNoActiveOperation()
    {
        if (_operations.Any(o => o.Status is PaymentOperationStatus.Pending or PaymentOperationStatus.Processing or PaymentOperationStatus.Unknown))
            throw new InvalidOperationException("The previous operation must be resolved before starting another one.");
    }
}
