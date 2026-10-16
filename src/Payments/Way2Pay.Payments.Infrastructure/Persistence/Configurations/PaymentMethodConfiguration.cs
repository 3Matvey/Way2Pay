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
        builder.OwnsMany(method => method.ProviderReferences, ConfigureReferences);
        builder.Navigation(method => method.ProviderReferences)
            .HasField("_providerReferences").UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureReferences(OwnedNavigationBuilder<PaymentMethod, ProviderPaymentMethodReference> builder)
    {
        builder.ToTable("PaymentMethodReferences");
        builder.WithOwner().HasForeignKey("PaymentId");
        builder.Property<Guid>("PaymentId");
        builder.Property(reference => reference.ProviderAccountId).ValueGeneratedNever();
        builder.Property(reference => reference.Reference).IsRequired();
        builder.HasKey("PaymentId", nameof(ProviderPaymentMethodReference.ProviderAccountId));
    }
}
