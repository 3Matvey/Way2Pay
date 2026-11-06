using Way2Pay.Payments.Application.Payments;
using Way2Pay.Payments.Application.Payments.StartPaymentOperation;
using Way2Pay.Payments.Domain.Payments.Operations;
using static Way2Pay.Payments.Application.Tests.Payments.PaymentTestData;

namespace Way2Pay.Payments.Application.Tests.Payments;

public sealed class StartPaymentOperationTests
{
    private readonly TestPaymentRepository _repository = new() { Payment = CreatePayment() };
    private readonly TestOperationStartStore _store = new();
    private readonly TestUnitOfWork _unitOfWork = new();

    private StartPaymentOperationRequest CreateRequest() => new(
        _repository.Payment!.MerchantId, _repository.Payment.Id, PaymentOperationType.Charge, 100, "BYN", "operation-key");

    private StartPaymentOperationUseCase CreateUseCase() =>
        new(_repository, new TestMerchantAccess(), _store, _unitOfWork);

    [Fact]
    public async Task Execute_WithNewKey_StagesOperationAndIdempotencyRecordInOneSave()
    {
        var result = await CreateUseCase().ExecuteAsync(CreateRequest());

        Assert.True(result.IsSuccess);
        Assert.Equal(Assert.Single(_repository.Payment!.Operations).Id, result.Value.OperationId);
        Assert.Equal(result.Value.OperationId, _store.Staged!.OperationId);
        Assert.Equal(1, _unitOfWork.SaveCalls);
    }

    [Theory]
    [InlineData("amount")]
    [InlineData("type")]
    [InlineData("payment")]
    public async Task Execute_WithReusedKeyAndDifferentParameters_ReturnsConflict(string changedField)
    {
        var request = CreateRequest();
        _store.Committed = new(request.MerchantId, request.IdempotencyKey, request.PaymentId,
            request.Type, Amount(), Guid.NewGuid());
        var changed = changedField switch
        {
            "amount" => request with { Amount = 101 },
            "type" => request with { Type = PaymentOperationType.Authorize },
            _ => request with { PaymentId = Guid.NewGuid() }
        };
        var result = await CreateUseCase().ExecuteAsync(changed);

        Assert.False(result.IsSuccess);
        Assert.Equal("Payments.IdempotencyConflict", result.Error!.Code);
        Assert.Equal(0, _repository.ReadCalls);
        Assert.Equal(0, _unitOfWork.SaveCalls);
    }

    [Fact]
    public async Task Execute_WhenSaveConflicts_ReplaysCommittedOperationAfterDiscard()
    {
        var request = CreateRequest();
        var winningId = Guid.NewGuid();
        _unitOfWork.OnSave = _ => throw new PaymentConcurrencyException(new InvalidOperationException("Concurrent update"));
        _unitOfWork.OnDiscard = () => _store.Committed = new(
            request.MerchantId, request.IdempotencyKey, request.PaymentId, request.Type, Amount(), winningId);
        var result = await CreateUseCase().ExecuteAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(winningId, result.Value.OperationId);
        Assert.Equal(1, _unitOfWork.SaveCalls);
        Assert.Equal(1, _unitOfWork.DiscardCalls);
    }
}
