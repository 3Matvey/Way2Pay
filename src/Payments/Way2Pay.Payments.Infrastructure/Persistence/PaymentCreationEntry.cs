using Way2Pay.Payments.Domain.ValueObjects;

namespace Way2Pay.Payments.Infrastructure.Persistence;

internal sealed class PaymentCreationEntry
{
    public Guid MerchantId { get; init; }
    public required string Key { get; init; }
    public Guid PaymentId { get; init; }
    public decimal Amount { get; init; }
    public required string Currency { get; init; }
    public PaymentMethodSourceKind SourceKind { get; init; }
    public Guid SourceId { get; init; }
}
