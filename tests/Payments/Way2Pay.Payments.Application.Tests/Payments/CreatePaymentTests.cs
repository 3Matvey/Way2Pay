using Way2Pay.Payments.Application.Payments.CreatePayment;
using Way2Pay.Payments.Domain.ValueObjects;
using static Way2Pay.Payments.Application.Tests.Payments.PaymentTestData;

namespace Way2Pay.Payments.Application.Tests.Payments;

public sealed class CreatePaymentTests
{
    private readonly TestPaymentRepository _repository = new();
    private readonly TestCreationStore _store = new();
    private readonly TestUnitOfWork _unitOfWork = new();
    private readonly CreatePaymentRequest _request;

    public CreatePaymentTests()
    {
        var payment = CreatePayment();
        _request = new(payment.MerchantId, 100, "BYN", "create-key", payment.PaymentMethod);
    }

    private CreatePaymentUseCase CreateUseCase() =>
        new(_repository, new TestMerchantAccess(), _store, _unitOfWork);

    [Fact]
    public async Task Execute_WithNewKey_StagesPaymentAndIdempotencyRecordInOneSave()
    {
        var result = await CreateUseCase().ExecuteAsync(_request);

        Assert.True(result.IsSuccess);
        var payment = Assert.Single(_repository.Added);
        Assert.Equal(payment.Id, result.Value.PaymentId);
        Assert.Equal(payment.Id, _store.Staged!.PaymentId);
        Assert.Equal(_request.PaymentMethod, _store.Staged.PaymentMethod);
        Assert.Equal(1, _unitOfWork.SaveCalls);
    }

    [Fact]
    public async Task Execute_WithSameKeyAndParameters_ReplaysWithoutCreatingOrSaving()
    {
        var existingId = Guid.NewGuid();
        _store.Committed = new(_request.MerchantId, _request.IdempotencyKey, Amount(), existingId, _request.PaymentMethod);
        var result = await CreateUseCase().ExecuteAsync(_request);

        Assert.True(result.IsSuccess);
        Assert.Equal(existingId, result.Value.PaymentId);
        Assert.Empty(_repository.Added);
        Assert.Equal(0, _unitOfWork.SaveCalls);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Execute_WithReusedKeyAndDifferentParameters_ReturnsConflict(bool changeAmount)
    {
        _store.Committed = new(_request.MerchantId, _request.IdempotencyKey, Amount(), Guid.NewGuid(), _request.PaymentMethod);
        var changed = changeAmount ? _request with { Amount = 101 }
            : _request with { PaymentMethod = PaymentMethod.FromProviderToken(_request.PaymentMethod.SourceId) };
        var result = await CreateUseCase().ExecuteAsync(changed);

        Assert.False(result.IsSuccess);
        Assert.Equal("Payments.IdempotencyConflict", result.Error!.Code);
        Assert.Empty(_repository.Added);
        Assert.Equal(0, _unitOfWork.SaveCalls);
    }

    [Fact]
    public async Task Execute_WhenConcurrentCreationWins_DiscardsAndReturnsCommittedPayment()
    {
        var winningId = Guid.NewGuid();
        _unitOfWork.OnSave = _ => throw new PaymentCreationConflictException();
        _unitOfWork.OnDiscard = () => _store.Committed = new(
            _request.MerchantId, _request.IdempotencyKey, Amount(), winningId, _request.PaymentMethod);
        var result = await CreateUseCase().ExecuteAsync(_request);

        Assert.True(result.IsSuccess);
        Assert.Equal(winningId, result.Value.PaymentId);
        Assert.NotEqual(winningId, Assert.Single(_repository.Added).Id);
        Assert.Equal(1, _unitOfWork.SaveCalls);
        Assert.Equal(1, _unitOfWork.DiscardCalls);
    }
}
