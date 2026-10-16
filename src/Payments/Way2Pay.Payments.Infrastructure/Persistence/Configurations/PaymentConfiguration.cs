using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Way2Pay.Payments.Domain.Payments;

namespace Way2Pay.Payments.Infrastructure.Persistence.Configurations;

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        ConfigureTable(builder);
        builder.HasKey(payment => payment.Id);
        builder.Property(payment => payment.Id).ValueGeneratedNever();
        builder.Property(payment => payment.Version).IsConcurrencyToken();
        builder.Ignore(payment => payment.Currency);
        builder.HasIndex(payment => new { payment.MerchantId, payment.CreatedAt });
        builder.ComplexProperty(payment => payment.Amount, money => MoneyConfiguration.Configure(money, "Amount"));
        builder.ComplexProperty(payment => payment.Balance, PaymentBalanceConfiguration.Configure);
        ConfigurePaymentMethod(builder);
        ConfigureOperations(builder);
    }

    private static void ConfigureTable(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments", table =>
        {
            table.HasCheckConstraint("CK_Payments_Amount", "[Amount] > 0");
            table.HasCheckConstraint("CK_Payments_Balance",
                "[AuthorizedAmount] >= 0 AND [CapturedAmount] >= 0 AND [RefundedAmount] >= 0 AND [RefundedAmount] <= [CapturedAmount]");
        });
    }

    private static void ConfigurePaymentMethod(EntityTypeBuilder<Payment> builder)
    {
        builder.OwnsOne(payment => payment.PaymentMethod, PaymentMethodConfiguration.Configure);
        builder.Navigation(payment => payment.PaymentMethod).IsRequired();
    }

    private static void ConfigureOperations(EntityTypeBuilder<Payment> builder)
    {
        builder.HasMany(payment => payment.Operations).WithOne()
            .HasForeignKey(operation => operation.PaymentId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(payment => payment.Operations)
            .HasField("_operations").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
