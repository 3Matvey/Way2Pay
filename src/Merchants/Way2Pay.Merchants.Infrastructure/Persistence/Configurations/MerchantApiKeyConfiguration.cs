using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Way2Pay.Merchants.Domain.ApiKeys;
using Way2Pay.Merchants.Domain.Merchants;

namespace Way2Pay.Merchants.Infrastructure.Persistence.Configurations;

internal sealed class MerchantApiKeyConfiguration : IEntityTypeConfiguration<MerchantApiKey>
{
    public void Configure(EntityTypeBuilder<MerchantApiKey> builder)
    {
        ConfigureTable(builder);
        builder.HasKey(apiKey => apiKey.Id);
        builder.Property(apiKey => apiKey.Id).ValueGeneratedNever();
        builder.Property(apiKey => apiKey.Name).HasMaxLength(200).IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(apiKey => apiKey.SecretHash).HasColumnType("char(64)").IsRequired();
        builder.Property(apiKey => apiKey.RevokedAt).IsConcurrencyToken();
        builder.HasIndex(apiKey => apiKey.MerchantId);
        builder.HasOne<Merchant>().WithMany().HasForeignKey(apiKey => apiKey.MerchantId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureTable(EntityTypeBuilder<MerchantApiKey> builder)
    {
        builder.ToTable("MerchantApiKeys", table =>
        {
            table.HasCheckConstraint("CK_MerchantApiKeys_Expiration", "[ExpiresAt] IS NULL OR [ExpiresAt] > [CreatedAt]");
            table.HasCheckConstraint("CK_MerchantApiKeys_Revocation", "[RevokedAt] IS NULL OR [RevokedAt] >= [CreatedAt]");
        });
    }
}
