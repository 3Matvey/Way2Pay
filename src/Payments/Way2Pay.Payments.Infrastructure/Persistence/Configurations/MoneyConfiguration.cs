using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Way2Pay.Payments.Domain.ValueObjects;

namespace Way2Pay.Payments.Infrastructure.Persistence.Configurations;

internal static class MoneyConfiguration
{
    /// <summary>Maps a monetary value to decimal(19,4) and a three-letter currency column.</summary>
    /// <remarks>Payment amounts and balances explicitly share the same currency column.</remarks>
    internal static void Configure(ComplexPropertyBuilder<Money> builder, string amountColumn)
    {
        builder.IsRequired();
        builder.Property(money => money.Amount).HasColumnName(amountColumn).HasPrecision(19, 4);
        builder.Property(money => money.Currency)
            .HasConversion(currency => currency.Value, value => new CurrencyCode(value))
            .HasColumnName("Currency").HasColumnType("char(3)").IsUnicode(false).IsRequired();
    }
}
