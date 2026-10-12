namespace Way2Pay.Routing.Contracts.Conditions;

/// <summary>Tests whether the payment method kind belongs to or is excluded from a non-empty method set.</summary>
public sealed record PaymentMethodCondition : PaymentRoutingCondition
{
    public SetMembershipOperator Operator { get; }
    public IReadOnlyList<RoutingPaymentMethodType> PaymentMethods { get; }

    public PaymentMethodCondition(SetMembershipOperator @operator, IEnumerable<RoutingPaymentMethodType> paymentMethods)
    {
        ArgumentNullException.ThrowIfNull(paymentMethods);
        var snapshot = paymentMethods.ToArray();
        if (snapshot.Length == 0)
            throw new ArgumentException("At least one payment method is required.", nameof(paymentMethods));
        Operator = @operator;
        PaymentMethods = Array.AsReadOnly(snapshot);
    }
}
