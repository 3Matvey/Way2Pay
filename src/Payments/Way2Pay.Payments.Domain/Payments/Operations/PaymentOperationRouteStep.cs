using Way2Pay.Payments.Domain.Common;

namespace Way2Pay.Payments.Domain.Payments.Operations;

/// <summary>A provider account at a fixed position in an operation's saved route.</summary>
/// <remarks>Execution outcomes belong to attempts referencing this step, not to the step itself.</remarks>
public sealed class PaymentOperationRouteStep : Entity
{
    public Guid PaymentOperationId { get; }
    public int Position { get; }
    public Guid ProviderAccountId { get; }

    internal PaymentOperationRouteStep(Guid operationId, int position, Guid providerAccountId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(position);
        PaymentOperationId = Guard.RequiredId(operationId);
        Position = position;
        ProviderAccountId = Guard.RequiredId(providerAccountId);
    }
}
