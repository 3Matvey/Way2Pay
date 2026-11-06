using Way2Pay.Common.Results;
using Way2Pay.Payments.Application.Payments.Configuration;
using Way2Pay.Payments.Application.Payments.ExecutePaymentOperation;
using Way2Pay.Payments.Application.Payments.PaymentMethods;
using Way2Pay.Payments.Application.Payments.Providers;
using Way2Pay.Payments.Application.Payments.Routing;
using Way2Pay.Payments.Application.Routing;
using Way2Pay.Payments.Domain.Payments;
using Way2Pay.Payments.Domain.Payments.Operations;
using Way2Pay.Payments.Domain.ValueObjects;
using Way2Pay.Routing.Contracts;

namespace Way2Pay.Payments.Application.Tests.Payments;

internal sealed class ExecutionTestContext
{
    public static readonly Guid FirstProvider = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid SecondProvider = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid ThirdProvider = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public Payment Payment { get; }
    public PaymentOperation Operation { get; }
    public TestPaymentRepository Repository { get; } = new();
    public TestUnitOfWork UnitOfWork { get; } = new();
    public TestConfigurationProvider Configuration { get; } = new();
    public TestMethodAvailability Availability { get; } = new();
    public TestProviderGateway Provider { get; } = new();
    public TestRoutingEngine Routing { get; } = new();

    public ExecutionTestContext(PaymentMethod? method = null)
    {
        Payment = new Payment(Guid.NewGuid(), PaymentTestData.Amount(), method ?? PaymentMethod.FromVaultCard(Guid.NewGuid()));
        Operation = Payment.StartOperation(PaymentOperationType.Charge, Payment.Amount);
        Repository.Payment = Payment;
        Configuration.Snapshot = CreateConfiguration();
    }

    public Task<Result<ExecutePaymentOperationResponse>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var useCase = new ExecutePaymentOperationUseCase(Repository, Configuration, Availability,
            Routing, Provider, UnitOfWork, new FixedTimeProvider());
        return useCase.ExecuteAsync(new(Payment.MerchantId, Payment.Id, Operation.Id), cancellationToken);
    }

    public MerchantPaymentConfiguration CreateConfiguration(bool firstActive = true, long version = 1) =>
        new(Payment.MerchantId, version,
            [CreateAccount(FirstProvider, firstActive), CreateAccount(SecondProvider), CreateAccount(ThirdProvider)],
            new PaymentRoutingPolicy(version, [], [FirstProvider, SecondProvider, ThirdProvider]));

    private static ProviderAccountConfiguration CreateAccount(Guid id, bool active = true) =>
        new(id, "test-provider", active, [PaymentOperationType.Charge], [new CurrencyCode("BYN")],
            [PaymentMethodType.Card], [PaymentMethodSourceKind.VaultCard, PaymentMethodSourceKind.ProviderToken]);

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => PaymentTestData.Now;
    }
}

internal sealed class TestConfigurationProvider : IMerchantPaymentConfigurationProvider
{
    public MerchantPaymentConfiguration? Snapshot { get; set; }
    public int Calls { get; private set; }

    public Task<MerchantPaymentConfiguration?> GetAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        Calls++;
        return Task.FromResult(Snapshot);
    }
}

internal sealed class TestMethodAvailability : IPaymentMethodAvailabilityProvider
{
    public PaymentMethodAvailability? Result { get; set; } = new(true, null);

    public Task<PaymentMethodAvailability?> GetAsync(
        Guid merchantId, PaymentMethod paymentMethod, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result);
}

internal sealed class TestProviderGateway : IPaymentProviderGateway
{
    public List<ProviderOperationRequest> Requests { get; } = [];
    public ProviderOperationResult Result { get; set; } = ProviderOperationResult.Succeeded("provider-tx");
    public Func<ProviderOperationRequest, CancellationToken, Task<ProviderOperationResult>>? OnExecute { get; set; }

    public Task<ProviderOperationResult> ExecuteAsync(ProviderOperationRequest request, CancellationToken cancellationToken = default)
    {
        Requests.Add(request);
        return OnExecute is null ? Task.FromResult(Result) : OnExecute(request, cancellationToken);
    }
}

internal sealed class TestRoutingEngine : IPaymentRoutingEngine
{
    public int Calls { get; private set; }

    public PaymentRoutingDecision Evaluate(
        PaymentRoutingContext context, PaymentRoutingPolicy policy, CancellationToken cancellationToken = default)
    {
        Calls++;
        return new PaymentRoutingEngine().Evaluate(context, policy, cancellationToken);
    }
}
