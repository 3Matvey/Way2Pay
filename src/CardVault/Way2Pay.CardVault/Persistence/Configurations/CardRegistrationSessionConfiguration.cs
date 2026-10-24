using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Way2Pay.CardVault.CardRegistrationSessions;
using Way2Pay.CardVault.Cards;

namespace Way2Pay.CardVault.Persistence.Configurations;

internal sealed class CardRegistrationSessionConfiguration : IEntityTypeConfiguration<CardRegistrationSession>
{
    public void Configure(EntityTypeBuilder<CardRegistrationSession> builder)
    {
        ConfigureTable(builder);
        builder.HasKey(session => session.Id);
        builder.Property(session => session.Id).ValueGeneratedNever();
        builder.Property(session => session.ExternalCustomerId).HasMaxLength(450)
            .UsePropertyAccessMode(PropertyAccessMode.Property);
        builder.Property(session => session.TokenHash).HasColumnType("char(64)").IsRequired();
        builder.HasIndex(session => session.TokenHash).IsUnique();
        builder.Property(session => session.Version).IsConcurrencyToken();
        builder.HasIndex(session => session.ExpiresAt);
        builder.HasOne<StoredCard>().WithMany().HasForeignKey(session => session.StoredCardId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureTable(EntityTypeBuilder<CardRegistrationSession> builder)
    {
        builder.ToTable("CardRegistrationSessions", table =>
        {
            table.HasCheckConstraint("CK_CardRegistrationSessions_Expiration", "[ExpiresAt] > [CreatedAt]");
            table.HasCheckConstraint("CK_CardRegistrationSessions_Result",
                "([Status] = 1 AND [StoredCardId] IS NOT NULL) OR ([Status] IN (0, 2) AND [StoredCardId] IS NULL)");
        });
    }
}
