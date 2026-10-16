using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Way2Pay.Payments.Domain.Payments.Operations;

namespace Way2Pay.Payments.Infrastructure.Persistence.Configurations;

internal sealed class PaymentOperationConfiguration : IEntityTypeConfiguration<PaymentOperation>
{
    public void Configure(EntityTypeBuilder<PaymentOperation> builder)
    {
        builder.ToTable("PaymentOperations", table => table.HasCheckConstraint("CK_PaymentOperations_Amount", "[Amount] > 0"));
        builder.HasKey(operation => operation.Id);
        builder.Property(operation => operation.Id).ValueGeneratedNever();
        builder.HasIndex(operation => new { operation.PaymentId, operation.CreatedAt });
        builder.ComplexProperty(operation => operation.Amount, money => MoneyConfiguration.Configure(money, "Amount"));
        ConfigureActiveOperationIndex(builder);
        ConfigureRoute(builder);
        ConfigureAttempts(builder);
    }

    private static void ConfigureActiveOperationIndex(EntityTypeBuilder<PaymentOperation> builder)
    {
        builder.HasIndex(operation => operation.PaymentId).IsUnique()
            .HasDatabaseName("UX_PaymentOperations_ActivePayment")
            .HasFilter($"[Status] IN ({(int)PaymentOperationStatus.Pending}, {(int)PaymentOperationStatus.Processing}, {(int)PaymentOperationStatus.Unknown})");
    }

    private static void ConfigureRoute(EntityTypeBuilder<PaymentOperation> builder)
    {
        builder.HasOne(operation => operation.Route).WithOne()
            .HasForeignKey<PaymentOperationRoute>("PaymentOperationId").OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureAttempts(EntityTypeBuilder<PaymentOperation> builder)
    {
        builder.HasMany(operation => operation.Attempts).WithOne()
            .HasForeignKey(attempt => attempt.PaymentOperationId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(operation => operation.Attempts)
            .HasField("_attempts").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
