using Microsoft.EntityFrameworkCore;
using Way2Pay.Payments.Application.Payments.CreatePayment;
using Way2Pay.Payments.Domain.ValueObjects;

namespace Way2Pay.Payments.Infrastructure.Persistence.Repositories;

internal sealed class PaymentCreationStore(PaymentsDbContext dbContext) : IPaymentCreationStore
{
    public async Task<PaymentCreationRecord?> FindAsync(
        Guid merchantId, string key, CancellationToken cancellationToken = default)
    {
        var entry = await dbContext.Set<PaymentCreationEntry>().AsNoTracking()
            .SingleOrDefaultAsync(entry => entry.MerchantId == merchantId && entry.Key == key, cancellationToken);
        return entry is null ? null : ToRecord(entry);
    }

    public void Add(PaymentCreationRecord record) =>
        dbContext.Add(new PaymentCreationEntry
        {
            MerchantId = record.MerchantId,
            Key = record.Key,
            PaymentId = record.PaymentId,
            Amount = record.Amount.Amount,
            Currency = record.Amount.Currency.Value,
            SourceKind = record.PaymentMethod.SourceKind,
            SourceId = record.PaymentMethod.SourceId
        });

    private static PaymentCreationRecord ToRecord(PaymentCreationEntry entry)
    {
        var method = entry.SourceKind switch
        {
            PaymentMethodSourceKind.VaultCard => PaymentMethod.FromVaultCard(entry.SourceId),
            PaymentMethodSourceKind.ProviderToken => PaymentMethod.FromProviderToken(entry.SourceId),
            _ => throw new InvalidOperationException("The stored payment source kind is not supported.")
        };

        return new PaymentCreationRecord(entry.MerchantId, entry.Key,
            new Money(entry.Amount, new CurrencyCode(entry.Currency)), entry.PaymentId, method);
    }
}
