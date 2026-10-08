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
    /// <summary>The payment-method snapshot assigned when the payment is created.</summary>
    public PaymentMethod PaymentMethod { get; }
    public CurrencyCode Currency => Amount.Currency;
    public PaymentStatus Status { get; private set; }
    public PaymentBalance Balance { get; }
    public IReadOnlyList<PaymentOperation> Operations => _operations.AsReadOnly();

    public Payment(Guid merchantId, Money amount, PaymentMethod paymentMethod)
    {
        ArgumentNullException.ThrowIfNull(amount);
        ArgumentNullException.ThrowIfNull(paymentMethod);
        Guard.Positive(amount.Amount);
        MerchantId = Guard.RequiredId(merchantId);
        Amount = amount;
        PaymentMethod = paymentMethod;
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

    /// <summary>Creates an operation attempt through an eligible provider account.</summary>
    /// <remarks>
    /// Authorize and Charge require a payment-method binding to the selected account.
    /// Capture, Void and Refund use the account of the original successful operation.
    /// Application checks account availability and the merchant permission to use it.
    /// </remarks>
    public PaymentAttempt StartAttempt(Guid operationId, Guid providerAccountId)
    {
        var operation = GetActiveOperation(operationId);
        EnsureProviderAccountAllowed(operation, providerAccountId);

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

    /// <summary>Marks the current attempt as unknown and blocks new attempts until its outcome is resolved.</summary>
    /// <param name="operationId">An operation belonging to this payment.</param>
    /// <param name="attemptId">The current attempt of the operation.</param>
    /// <param name="providerTransactionId">The transaction identifier, if already supplied by the provider.</param>
    /// <remarks>Monetary balances remain unchanged. The attempt and operation remain incomplete.</remarks>
    public void MarkAttemptUnknown(Guid operationId, Guid attemptId, string? providerTransactionId = null)
    {
        var operation = GetActiveOperation(operationId);
        operation.GetCurrentAttempt(attemptId).MarkUnknown(providerTransactionId);
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

    private void EnsureProviderAccountAllowed(PaymentOperation operation, Guid providerAccountId)
    {
        var allowed = operation.Type is PaymentOperationType.Authorize or PaymentOperationType.Charge
            ? PaymentMethod.ProviderReferences.Any(reference => reference.ProviderAccountId == providerAccountId)
            : GetOriginalAttempt(operation.Type)?.ProviderAccountId == providerAccountId;
        if (!allowed)
            throw new InvalidOperationException("The provider account is not allowed for this operation.");
    }

    private void EnsureOperationAllowed(PaymentOperationType type, Money amount)
    {
        ArgumentNullException.ThrowIfNull(amount);
        Guard.Positive(amount.Amount);
        if (amount.Currency != Currency)
            throw new PaymentOperationNotAllowedException("The operation must use the payment currency.");
        if (!CanStartOperation(type, amount))
            throw new PaymentOperationNotAllowedException("The operation or amount is not allowed for the current payment state.");
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
            throw new PaymentOperationNotAllowedException("The previous operation must be resolved before starting another one.");
    }
}
