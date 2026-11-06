using System.Globalization;
using Way2Pay.Common.Results;
using Way2Pay.Payments.Application.Payments.CreatePayment;
using Way2Pay.Payments.Application.Payments.StartPaymentOperation;
using Way2Pay.Payments.Domain.Payments.Operations;
using static Way2Pay.Payments.Application.Tests.Payments.PaymentTestData;

namespace Way2Pay.Payments.Application.Tests.Payments;

public sealed class PaymentAmountValidationTests
{
    [Theory]
    [InlineData("0.0001")]
    [InlineData("999999999999999.9999")]
    [InlineData("1.23000")]
    public async Task Create_WhenAmountHasValidPrecision_PreservesValue(string value)
    {
        var repository = new TestPaymentRepository { Payment = CreatePayment() };
        var unitOfWork = new TestUnitOfWork();
        var amount = decimal.Parse(value, CultureInfo.InvariantCulture);
        var result = await ExecuteCreateAsync(repository, unitOfWork, amount);

        Assert.True(result.IsSuccess);
        Assert.Equal(amount, Assert.Single(repository.Added).Amount.Amount);
        Assert.Equal(1, unitOfWork.SaveCalls);
    }

    // Reject precision loss instead of silently rounding the merchant's amount.
    [Theory]
    [InlineData("0.00001")]
    [InlineData("100.12345")]
    public async Task Create_WhenAmountHasExcessPrecision_RejectsBeforeStagingOrSaving(string value)
    {
        var repository = new TestPaymentRepository { Payment = CreatePayment() };
        var unitOfWork = new TestUnitOfWork();
        var amount = decimal.Parse(value, CultureInfo.InvariantCulture);
        var result = await ExecuteCreateAsync(repository, unitOfWork, amount);

        Assert.False(result.IsSuccess);
        Assert.Equal("Payments.InvalidAmount", result.Error!.Code);
        Assert.Empty(repository.Added);
        Assert.Equal(0, unitOfWork.SaveCalls);
    }

    [Theory]
    [InlineData("0.00001")]
    [InlineData("10.12345")]
    public async Task StartRefund_WhenAmountWouldBeRounded_RejectsBeforeAddingOperation(string value)
    {
        var payment = CreateCapturedPayment();
        var repository = new TestPaymentRepository { Payment = payment };
        var unitOfWork = new TestUnitOfWork();
        var useCase = new StartPaymentOperationUseCase(repository, new TestMerchantAccess(), new TestOperationStartStore(), unitOfWork);
        var amount = decimal.Parse(value, CultureInfo.InvariantCulture);
        var result = await useCase.ExecuteAsync(new(
            payment.MerchantId, payment.Id, PaymentOperationType.Refund, amount, "BYN", "refund-key"));

        Assert.False(result.IsSuccess);
        Assert.Equal("Payments.InvalidAmount", result.Error!.Code);
        Assert.Single(payment.Operations);
        Assert.Equal(0, unitOfWork.SaveCalls);
    }

    [Fact]
    public async Task Create_WhenCurrencyIsInvalid_ReturnsCurrencyErrorWithoutSaving()
    {
        var repository = new TestPaymentRepository { Payment = CreatePayment() };
        var unitOfWork = new TestUnitOfWork();
        var result = await ExecuteCreateAsync(repository, unitOfWork, 100, "byn");

        Assert.False(result.IsSuccess);
        Assert.Equal("Payments.InvalidCurrency", result.Error!.Code);
        Assert.Empty(repository.Added);
        Assert.Equal(0, unitOfWork.SaveCalls);
    }

    private static async Task<Result> ExecuteCreateAsync(
        TestPaymentRepository repository, TestUnitOfWork unitOfWork, decimal amount, string currency = "BYN")
    {
        var payment = repository.Payment!;
        var useCase = new CreatePaymentUseCase(repository, new TestMerchantAccess(), new TestCreationStore(), unitOfWork);
        return await useCase.ExecuteAsync(new(payment.MerchantId, amount, currency, "key", payment.PaymentMethod));
    }
}
