using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Way2Pay.Merchants.Domain.Merchants;

namespace Way2Pay.Merchants.Infrastructure.Persistence.Configurations;

internal sealed class MerchantConfiguration : IEntityTypeConfiguration<Merchant>
{
    public void Configure(EntityTypeBuilder<Merchant> builder)
    {
        builder.ToTable("Merchants", table =>
            table.HasCheckConstraint("CK_Merchants_Status", "[Status] IN (0, 1)"));
        builder.HasKey(merchant => merchant.Id);
        builder.Property(merchant => merchant.Id).ValueGeneratedNever();
        builder.Property(merchant => merchant.Name).HasMaxLength(200).IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Property);
    }
}
