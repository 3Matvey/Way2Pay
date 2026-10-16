using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Way2Pay.Payments.Domain.Payments.Attempts;

namespace Way2Pay.Payments.Infrastructure.Persistence.Configurations;

internal sealed class PaymentAttemptConfiguration : IEntityTypeConfiguration<PaymentAttempt>
{
    public void Configure(EntityTypeBuilder<PaymentAttempt> builder)
    {
        builder.ToTable("PaymentAttempts", table => table.HasCheckConstraint("CK_PaymentAttempts_Number", "[Number] > 0"));
        builder.HasKey(attempt => attempt.Id);
        builder.Property(attempt => attempt.Id).ValueGeneratedNever();
        builder.Ignore(attempt => attempt.ProviderAccountId);
        builder.HasIndex(attempt => new { attempt.PaymentOperationId, attempt.Number }).IsUnique();
        ConfigureRouteStep(builder);
    }

    private static void ConfigureRouteStep(EntityTypeBuilder<PaymentAttempt> builder)
    {
        // The composite foreign key also prevents referencing a step from another operation.
        builder.HasOne(attempt => attempt.RouteStep).WithMany()
            .HasForeignKey(attempt => new { attempt.RouteStepId, attempt.PaymentOperationId })
            .HasPrincipalKey(step => new { step.Id, step.PaymentOperationId })
            .OnDelete(DeleteBehavior.NoAction);
    }
}
