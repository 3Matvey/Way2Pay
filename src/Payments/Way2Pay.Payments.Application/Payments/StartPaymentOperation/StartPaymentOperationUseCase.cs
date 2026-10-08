using Way2Pay.Payments.Application.Common.Interfaces;
using Way2Pay.Payments.Application.Common.Results;
using Way2Pay.Payments.Domain.Payments;
using Way2Pay.Payments.Domain.Payments.Operations;
using Way2Pay.Payments.Domain.ValueObjects;

namespace Way2Pay.Payments.Application.Payments.StartPaymentOperation;

public sealed class StartPaymentOperationUseCase(
    IPaymentRepository paymentRepository,
    IMerchantPaymentAccess merchantPaymentAccess,
    IPaymentOperationStartStore operationStartStore,
    IUnitOfWork unitOfWork) : IUseCase
{
    public async Task<Result<StartPaymentOperationResponse>> ExecuteAsync(
        StartPaymentOperationRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var amount = ValidateRequest(request);
        if (!amount.IsSuccess)
            return amount.Error;
        if (!await merchantPaymentAccess.CanAcceptPaymentsAsync(request.MerchantId, cancellationToken))
            return PaymentErrors.MerchantUnavailable();
        return await StartOrReplayAsync(request, amount.Value, cancellationToken);
    }

    private static Result<Money> ValidateRequest(StartPaymentOperationRequest request)
    {
        if (request.MerchantId == Guid.Empty)
            return PaymentErrors.InvalidMerchantId();
        if (request.PaymentId == Guid.Empty)
            return PaymentErrors.InvalidPaymentId();
        if (!Enum.IsDefined(request.Type))
            return PaymentErrors.InvalidOperationType();
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
            return PaymentErrors.InvalidIdempotencyKey();
        return PaymentAmountFactory.Create(request.Amount, request.Currency);
    }

    private async Task<Result<StartPaymentOperationResponse>> StartOrReplayAsync(
        StartPaymentOperationRequest request, Money amount, CancellationToken cancellationToken)
    {
        var record = await operationStartStore.FindAsync(
            request.MerchantId, request.IdempotencyKey, cancellationToken);
        if (record is not null)
            return Replay(record, request, amount);
        return await StartAsync(request, amount, cancellationToken);
    }

    private async Task<Result<StartPaymentOperationResponse>> StartAsync(
        StartPaymentOperationRequest request, Money amount, CancellationToken cancellationToken)
    {
        var payment = await paymentRepository.GetByIdAsync(request.MerchantId, request.PaymentId, cancellationToken);
        if (payment is null)
            return PaymentErrors.NotFound();
        var operation = StartOperation(payment, request.Type, amount);
        if (!operation.IsSuccess)
            return await ResolveRejectedOperationAsync(request, amount, cancellationToken);
        operationStartStore.Add(new PaymentOperationStartRecord(
            request.MerchantId, request.IdempotencyKey, payment.Id, request.Type, amount, operation.Value.Id));
        return await SaveAsync(request, amount, operation.Value.Id, cancellationToken);
    }

    private static Result<PaymentOperation> StartOperation(Payment payment, PaymentOperationType type, Money amount)
    {
        try
        {
            return payment.StartOperation(type, amount);
        }
        catch (PaymentOperationNotAllowedException)
        {
            return PaymentErrors.OperationNotAllowed();
        }
    }

    private async Task<Result<StartPaymentOperationResponse>> ResolveRejectedOperationAsync(
        StartPaymentOperationRequest request, Money amount, CancellationToken cancellationToken)
    {
        var record = await operationStartStore.FindAsync(
            request.MerchantId, request.IdempotencyKey, cancellationToken);
        if (record is not null)
            return Replay(record, request, amount);
        return PaymentErrors.OperationNotAllowed();
    }

    private async Task<Result<StartPaymentOperationResponse>> SaveAsync(
        StartPaymentOperationRequest request, Money amount, Guid operationId, CancellationToken cancellationToken)
    {
        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return new StartPaymentOperationResponse(request.PaymentId, operationId, PaymentOperationStatus.Pending);
        }
        catch (Exception exception) when (exception is PaymentOperationStartConflictException or PaymentConcurrencyException)
        {
            return await ResolveSaveConflictAsync(request, amount, exception, cancellationToken);
        }
    }

    private async Task<Result<StartPaymentOperationResponse>> ResolveSaveConflictAsync(
        StartPaymentOperationRequest request, Money amount, Exception exception, CancellationToken cancellationToken)
    {
        await unitOfWork.DiscardChangesAsync(cancellationToken);
        var record = await operationStartStore.FindAsync(
            request.MerchantId, request.IdempotencyKey, cancellationToken);
        if (record is not null)
            return Replay(record, request, amount);
        if (exception is PaymentOperationStartConflictException)
            throw new InvalidOperationException("The conflicting committed operation start was not found.", exception);
        return PaymentErrors.ConcurrencyConflict();
    }

    private static Result<StartPaymentOperationResponse> Replay(
        PaymentOperationStartRecord record, StartPaymentOperationRequest request, Money amount)
    {
        if (record.PaymentId != request.PaymentId || record.Type != request.Type || record.Amount != amount)
            return PaymentErrors.IdempotencyConflict();
        return new StartPaymentOperationResponse(record.PaymentId, record.OperationId, PaymentOperationStatus.Pending);
    }
}
