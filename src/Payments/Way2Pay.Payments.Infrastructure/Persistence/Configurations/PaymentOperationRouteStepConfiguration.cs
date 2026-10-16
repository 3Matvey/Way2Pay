using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Way2Pay.Payments.Domain.Payments.Operations;

namespace Way2Pay.Payments.Infrastructure.Persistence.Configurations;

internal sealed class PaymentOperationRouteStepConfiguration : IEntityTypeConfiguration<PaymentOperationRouteStep>
{
    public void Configure(EntityTypeBuilder<PaymentOperationRouteStep> builder)
    {
        builder.ToTable("PaymentOperationRouteSteps", table => table.HasCheckConstraint("CK_PaymentOperationRouteSteps_Position", "[Position] > 0"));
        builder.HasKey(step => step.Id);
        builder.Property(step => step.Id).ValueGeneratedNever();
        builder.Property(step => step.PaymentOperationId);
        builder.Property(step => step.Position);
        builder.Property(step => step.ProviderAccountId);
        builder.HasAlternateKey(step => new { step.Id, step.PaymentOperationId });
        builder.HasIndex(step => new { step.PaymentOperationId, step.Position }).IsUnique();
        builder.HasIndex(step => new { step.PaymentOperationId, step.ProviderAccountId }).IsUnique();
    }
}
