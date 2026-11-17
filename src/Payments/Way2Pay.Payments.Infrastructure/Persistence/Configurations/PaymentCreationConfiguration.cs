using System.Buffers.Binary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Way2Pay.Payments.Domain.Payments;

namespace Way2Pay.Payments.Infrastructure.Persistence.Configurations;

internal sealed class PaymentCreationConfiguration : IEntityTypeConfiguration<PaymentCreationEntry>
{
    internal const string PrimaryKeyName = "PK_PaymentCreations";

    public void Configure(EntityTypeBuilder<PaymentCreationEntry> builder)
    {
        builder.ToTable("PaymentCreations");
        builder.HasKey(entry => new { entry.MerchantId, entry.Key }).HasName(PrimaryKeyName);
        builder.Property(entry => entry.Key).HasColumnName("IdempotencyKey").HasColumnType("varbinary(512)")
            .HasConversion(key => EncodeKey(key), bytes => DecodeKey(bytes));
        builder.Property(entry => entry.Amount).HasPrecision(19, 4);
        builder.Property(entry => entry.Currency).HasColumnType("char(3)").IsUnicode(false).IsRequired();
        builder.HasOne<Payment>().WithMany().HasForeignKey(entry => entry.PaymentId).OnDelete(DeleteBehavior.Restrict);
    }

    // Preserve every UTF-16 code unit and distinguish keys ending in zero bytes.
    private static byte[] EncodeKey(string key)
    {
        var bytes = new byte[2 + key.Length * 2];
        BinaryPrimitives.WriteUInt16LittleEndian(bytes, checked((ushort)key.Length));
        for (var index = 0; index < key.Length; index++)
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(2 + index * 2), key[index]);
        return bytes;
    }

    private static string DecodeKey(byte[] bytes) =>
        string.Create(BinaryPrimitives.ReadUInt16LittleEndian(bytes), bytes, static (chars, source) =>
        {
            for (var index = 0; index < chars.Length; index++)
                chars[index] = (char)BinaryPrimitives.ReadUInt16LittleEndian(source.AsSpan(2 + index * 2));
        });
}
