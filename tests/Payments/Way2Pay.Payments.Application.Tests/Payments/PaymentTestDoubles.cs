using Way2Pay.Payments.Application.Common;
using Way2Pay.Payments.Application.Payments;
using Way2Pay.Payments.Application.Payments.CreatePayment;
using Way2Pay.Payments.Application.Payments.StartPaymentOperation;
using Way2Pay.Payments.Domain.Payments;
using Way2Pay.Payments.Domain.Payments.Operations;
using Way2Pay.Payments.Domain.ValueObjects;

namespace Way2Pay.Payments.Application.Tests.Payments;

internal static class PaymentTestData
{
    public static readonly DateTimeOffset Now = new(2026, 11, 6, 12, 0, 0, TimeSpan.Zero);
    public static Money Amount(decimal value = 100) => new(value, new CurrencyCode("BYN"));
    public static Payment CreatePayment() =>
        new(Guid.NewGuid(), Amount(), PaymentMethod.FromVaultCard(Guid.NewGuid()));

    public static Payment CreateCapturedPayment()
    {
        var payment = CreatePayment();
        var operation = payment.StartOperation(PaymentOperationType.Charge, payment.Amount);
        var providerId = Guid.NewGuid();
        payment.SetOperationRoute(operation.Id, 1, 1, [providerId], null, "Test charge route");
        var attempt = payment.StartAttempt(operation.Id, providerId);
        payment.RecordAttemptSuccess(operation.Id, attempt.Id, "captured-tx", Now);
        return payment;
    }
}

internal sealed class TestPaymentRepository : IPaymentRepository
{
    public Payment? Payment { get; set; }
    public List<Payment> Added { get; } = [];
    public int ReadCalls { get; private set; }

    public Task<Payment?> GetByIdAsync(Guid merchantId, Guid paymentId, CancellationToken cancellationToken = default)
    {
        ReadCalls++;
        return Task.FromResult(Payment is { } payment && payment.MerchantId == merchantId && payment.Id == paymentId
            ? payment : null);
    }

    public void Add(Payment payment) => Added.Add(payment);
}

/// <summary>Scripts save failures and records calls; it does not emulate database transactions or concurrency.</summary>
internal sealed class TestUnitOfWork : IUnitOfWork
{
    public int SaveCalls { get; private set; }
    public int DiscardCalls { get; private set; }
    public List<CancellationToken> SaveTokens { get; } = [];
    public Action<int>? OnSave { get; set; }
    public Action? OnDiscard { get; set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        SaveTokens.Add(cancellationToken);
        SaveCalls++;
        OnSave?.Invoke(SaveCalls);
        return Task.FromResult(1);
    }

    public Task DiscardChangesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        DiscardCalls++;
        OnDiscard?.Invoke();
        return Task.CompletedTask;
    }
}

internal sealed class TestMerchantAccess : IMerchantPaymentAccess
{
    public Task<bool> CanAcceptPaymentsAsync(Guid merchantId, CancellationToken cancellationToken = default) =>
        Task.FromResult(true);
}

internal sealed class TestCreationStore : IPaymentCreationStore
{
    public PaymentCreationRecord? Committed { get; set; }
    public PaymentCreationRecord? Staged { get; private set; }

    public Task<PaymentCreationRecord?> FindAsync(Guid merchantId, string key, CancellationToken cancellationToken = default) =>
        Task.FromResult(Committed is { } record && record.MerchantId == merchantId && record.Key == key ? record : null);

    public void Add(PaymentCreationRecord record) => Staged = record;
}

internal sealed class TestOperationStartStore : IPaymentOperationStartStore
{
    public PaymentOperationStartRecord? Committed { get; set; }
    public PaymentOperationStartRecord? Staged { get; private set; }

    public Task<PaymentOperationStartRecord?> FindAsync(Guid merchantId, string key, CancellationToken cancellationToken = default) =>
        Task.FromResult(Committed is { } record && record.MerchantId == merchantId && record.Key == key ? record : null);

    public void Add(PaymentOperationStartRecord record) => Staged = record;
}
