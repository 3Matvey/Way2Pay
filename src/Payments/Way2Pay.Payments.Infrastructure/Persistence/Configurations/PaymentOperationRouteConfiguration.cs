using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Way2Pay.Payments.Domain.Payments.Operations;

namespace Way2Pay.Payments.Infrastructure.Persistence.Configurations;

/// <summary>Maps a route to its own table using its operation ID as a shadow primary key.</summary>
internal sealed class PaymentOperationRouteConfiguration : IEntityTypeConfiguration<PaymentOperationRoute>
{
    public void Configure(EntityTypeBuilder<PaymentOperationRoute> builder)
    {
        builder.ToTable("PaymentOperationRoutes", table =>
            table.HasCheckConstraint("CK_PaymentOperationRoutes_Versions", "[ConfigurationVersion] > 0 AND ([PolicyVersion] IS NULL OR [PolicyVersion] > 0)"));
        builder.Property<Guid>("PaymentOperationId").ValueGeneratedNever();
        builder.HasKey("PaymentOperationId");
        builder.Property(route => route.Explanation).IsRequired();
        ConfigureSteps(builder);
    }

    private static void ConfigureSteps(EntityTypeBuilder<PaymentOperationRoute> builder)
    {
        builder.HasMany(route => route.Steps).WithOne()
            .HasForeignKey(step => step.PaymentOperationId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(route => route.Steps)
            .HasField("_steps").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
