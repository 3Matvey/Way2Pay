using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Way2Pay.Common.Results;
using Way2Pay.Payments.Application.Common;
using Way2Pay.Payments.Application.Payments;
using Way2Pay.Payments.Application.Payments.CreatePayment;
using Way2Pay.Payments.Domain.ValueObjects;
using Way2Pay.Payments.Infrastructure.Persistence;

namespace Way2Pay.Payments.IntegrationTests;

public sealed class CreatePaymentTests(SqlServerFixture database) : IClassFixture<SqlServerFixture>
{
    [Fact]
    public async Task Execute_WithConcurrentRequestsForSameKey_SavesOnePaymentAndReturnsSameId()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var request = new CreatePaymentRequest(Guid.NewGuid(), 12.50m, "USD", "order-42",
            PaymentMethod.FromVaultCard(Guid.NewGuid()));
        var gate = new CreationGate();
        var results = await Task.WhenAll(CreateAsync(request, gate, timeout.Token), CreateAsync(request, gate, timeout.Token));

        Assert.All(results, result => Assert.True(result.IsSuccess));
        Assert.Equal(results[0].Value.PaymentId, results[1].Value.PaymentId);
        await AssertSavedOnceAsync(request.MerchantId, results[0].Value.PaymentId, timeout.Token);
    }

    private async Task<Result<CreatePaymentResponse>> CreateAsync(
        CreatePaymentRequest request, CreationGate gate, CancellationToken cancellationToken)
    {
        await using var scope = database.CreateScope();
        var services = scope.ServiceProvider;
        var store = new SynchronizedStore(services.GetRequiredService<IPaymentCreationStore>(), gate);
        var useCase = new CreatePaymentUseCase(services.GetRequiredService<IPaymentRepository>(),
            new MerchantAccess(), store, services.GetRequiredService<IUnitOfWork>());
        return await useCase.ExecuteAsync(request, cancellationToken);
    }

    private async Task AssertSavedOnceAsync(Guid merchantId, Guid paymentId, CancellationToken cancellationToken)
    {
        await using var scope = database.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
        var payment = await db.Payments.SingleAsync(payment => payment.MerchantId == merchantId, cancellationToken);
        var entry = await db.Set<PaymentCreationEntry>()
            .SingleAsync(entry => entry.MerchantId == merchantId, cancellationToken);
        Assert.Equal(paymentId, payment.Id);
        Assert.Equal(paymentId, entry.PaymentId);
    }

    private sealed class MerchantAccess : IMerchantPaymentAccess
    {
        public Task<bool> CanAcceptPaymentsAsync(Guid merchantId, CancellationToken cancellationToken = default) =>
            Task.FromResult(true);
    }

    private sealed class SynchronizedStore(IPaymentCreationStore store, CreationGate gate) : IPaymentCreationStore
    {
        public async Task<PaymentCreationRecord?> FindAsync(
            Guid merchantId, string key, CancellationToken cancellationToken = default)
        {
            var record = await store.FindAsync(merchantId, key, cancellationToken);
            if (record is null)
                await gate.WaitForBothReadsAsync(cancellationToken);
            return record;
        }

        public void Add(PaymentCreationRecord record) => store.Add(record);
    }

    private sealed class CreationGate
    {
        private readonly TaskCompletionSource _bothRead = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _reads;

        // Both SQL lookups must miss before either request attempts an insert.
        internal Task WaitForBothReadsAsync(CancellationToken cancellationToken)
        {
            if (Interlocked.Increment(ref _reads) == 2)
                _bothRead.SetResult();
            return _bothRead.Task.WaitAsync(cancellationToken);
        }
    }
}
