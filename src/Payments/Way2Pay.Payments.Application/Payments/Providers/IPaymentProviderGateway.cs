namespace Way2Pay.Payments.Application.Payments.Providers;

/// <summary>Executes one monetary attempt through the selected provider account.</summary>
/// <remarks>
/// The caller must persist the attempt before invoking the gateway.
/// Adapters resolve connection settings and credentials by ProviderAccountId.
/// Ambiguous transport errors must produce Unknown rather than a confirmed Failed result.
/// Cancellation or an exception does not prove that the provider performed no monetary action.
/// </remarks>
public interface IPaymentProviderGateway
{
    Task<ProviderOperationResult> ExecuteAsync(
        ProviderOperationRequest request, CancellationToken cancellationToken = default);
}
