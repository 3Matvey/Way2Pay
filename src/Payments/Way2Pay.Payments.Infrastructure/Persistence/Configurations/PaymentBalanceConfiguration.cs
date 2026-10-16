using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Way2Pay.Payments.Domain.Payments;

namespace Way2Pay.Payments.Infrastructure.Persistence.Configurations;

internal static class PaymentBalanceConfiguration
{
    internal static void Configure(ComplexPropertyBuilder<PaymentBalance> builder)
    {
        builder.IsRequired();
        builder.Ignore(balance => balance.Refundable);
        builder.ComplexProperty(balance => balance.Authorized, money => MoneyConfiguration.Configure(money, "AuthorizedAmount"));
        builder.ComplexProperty(balance => balance.Captured, money => MoneyConfiguration.Configure(money, "CapturedAmount"));
        builder.ComplexProperty(balance => balance.Refunded, money => MoneyConfiguration.Configure(money, "RefundedAmount"));
    }
}
