namespace Way2Pay.Payments.Application.Payments.ExecutePaymentOperation;

/// <summary>Requests the next provider attempt for an existing operation.</summary>
/// <remarks>
/// MerchantId must come from the authenticated context. Repeated requests never resend an unresolved
/// attempt. After a confirmed failure, a new request may advance to another saved route step.
/// </remarks>
public sealed record ExecutePaymentOperationRequest(Guid MerchantId, Guid PaymentId, Guid OperationId);
