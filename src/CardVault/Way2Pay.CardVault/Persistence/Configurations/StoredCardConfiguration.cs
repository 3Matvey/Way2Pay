using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Way2Pay.CardVault.Cards;

namespace Way2Pay.CardVault.Persistence.Configurations;

internal sealed class StoredCardConfiguration : IEntityTypeConfiguration<StoredCard>
{
    public void Configure(EntityTypeBuilder<StoredCard> builder)
    {
        builder.ToTable("StoredCards", table => table.HasCheckConstraint(
            "CK_StoredCards_Expiration", "[ExpirationMonth] BETWEEN 1 AND 12 AND [ExpirationYear] BETWEEN 1 AND 9999"));
        builder.HasKey(card => card.Id);
        builder.Property(card => card.Id).ValueGeneratedNever();
        builder.Property(card => card.PanCiphertext).IsRequired();
        builder.Property(card => card.ExternalCustomerId).HasMaxLength(450)
            .UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(card => card.LastFour).HasColumnType("char(4)").IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.HasIndex(card => new { card.MerchantId, card.ExternalCustomerId });
        builder.OwnsOne(card => card.Expiration, ConfigureExpiration);
        builder.Navigation(card => card.Expiration).IsRequired();
    }

    private static void ConfigureExpiration(OwnedNavigationBuilder<StoredCard, CardExpiration> builder)
    {
        builder.WithOwner().HasForeignKey("StoredCardId");
        builder.Property<Guid>("StoredCardId").HasColumnName("Id");
        builder.Property(expiration => expiration.Month).HasColumnName("ExpirationMonth");
        builder.Property(expiration => expiration.Year).HasColumnName("ExpirationYear");
    }
}
