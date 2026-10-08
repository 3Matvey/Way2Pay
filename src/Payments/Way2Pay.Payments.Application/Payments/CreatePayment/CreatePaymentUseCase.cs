using Way2Pay.Payments.Application.Common.Interfaces;
using Way2Pay.Payments.Application.Common.Results;
using Way2Pay.Payments.Domain.Payments;
using Way2Pay.Payments.Domain.ValueObjects;

namespace Way2Pay.Payments.Application.Payments.CreatePayment;

public sealed class CreatePaymentUseCase(
    IPaymentRepository paymentRepository,
    IMerchantPaymentAccess merchantPaymentAccess,
    IPaymentCreationStore paymentCreationStore,
    IUnitOfWork unitOfWork) : IUseCase
{
    public async Task<Result<CreatePaymentResponse>> ExecuteAsync(
        CreatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var amount = ValidateRequest(request);
        if (!amount.IsSuccess)
            return amount.Error;
        if (!await merchantPaymentAccess.CanAcceptPaymentsAsync(request.MerchantId, cancellationToken))
            return PaymentErrors.MerchantUnavailable();
        return await CreateOrReplayAsync(request, amount.Value, cancellationToken);
    }

    private static Result<Money> ValidateRequest(CreatePaymentRequest request)
    {
        if (request.MerchantId == Guid.Empty)
            return PaymentErrors.InvalidMerchantId();
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
            return PaymentErrors.InvalidIdempotencyKey();
        if (request.PaymentMethod is null)
            return PaymentErrors.InvalidPaymentMethod();
        return PaymentAmountFactory.Create(request.Amount, request.Currency);
    }

    private async Task<Result<CreatePaymentResponse>> CreateOrReplayAsync(
        CreatePaymentRequest request, Money amount, CancellationToken cancellationToken)
    {
        var record = await paymentCreationStore.FindAsync(
            request.MerchantId, request.IdempotencyKey, cancellationToken);
        if (record is not null)
            return Replay(record, amount, request.PaymentMethod);
        return await CreateAsync(request, amount, cancellationToken);
    }

    private async Task<Result<CreatePaymentResponse>> CreateAsync(
        CreatePaymentRequest request, Money amount, CancellationToken cancellationToken)
    {
        var payment = new Payment(request.MerchantId, amount, request.PaymentMethod);
        paymentRepository.Add(payment);
        paymentCreationStore.Add(new PaymentCreationRecord(
            request.MerchantId, request.IdempotencyKey, amount, payment.Id, request.PaymentMethod));
        return await SaveAsync(request, payment, cancellationToken);
    }

    private async Task<Result<CreatePaymentResponse>> SaveAsync(
        CreatePaymentRequest request, Payment payment, CancellationToken cancellationToken)
    {
        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return new CreatePaymentResponse(payment.Id, PaymentStatus.Created);
        }
        catch (PaymentCreationConflictException)
        {
            return await ResolveConflictAsync(request, payment.Amount, cancellationToken);
        }
    }

    private async Task<Result<CreatePaymentResponse>> ResolveConflictAsync(
        CreatePaymentRequest request, Money amount, CancellationToken cancellationToken)
    {
        await unitOfWork.DiscardChangesAsync(cancellationToken);
        var record = await paymentCreationStore.FindAsync(
            request.MerchantId, request.IdempotencyKey, cancellationToken)
            ?? throw new InvalidOperationException("The conflicting committed payment creation was not found.");
        return Replay(record, amount, request.PaymentMethod);
    }

    private static Result<CreatePaymentResponse> Replay(
        PaymentCreationRecord record, Money amount, PaymentMethod paymentMethod)
    {
        if (record.Amount != amount || record.PaymentMethod != paymentMethod)
            return PaymentErrors.IdempotencyConflict();
        return new CreatePaymentResponse(record.PaymentId, PaymentStatus.Created);
    }
}
