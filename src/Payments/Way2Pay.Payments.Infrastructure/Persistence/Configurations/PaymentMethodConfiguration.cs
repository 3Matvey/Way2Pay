using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Way2Pay.Payments.Domain.Payments;
using Way2Pay.Payments.Domain.ValueObjects;

namespace Way2Pay.Payments.Infrastructure.Persistence.Configurations;

internal static class PaymentMethodConfiguration
{
    internal static void Configure(OwnedNavigationBuilder<Payment, PaymentMethod> builder)
    {
        builder.WithOwner().HasForeignKey("PaymentId");
        builder.Property<Guid>("PaymentId").HasColumnName("Id");
        builder.Property(method => method.Type).HasColumnName("PaymentMethodType");
        builder.Property(method => method.SourceKind).HasColumnName("PaymentMethodSourceKind");
        builder.Property(method => method.SourceId).HasColumnName("PaymentMethodSourceId");
    }
}
