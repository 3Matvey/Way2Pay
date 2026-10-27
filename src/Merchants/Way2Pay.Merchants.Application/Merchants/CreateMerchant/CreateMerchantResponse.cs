using Way2Pay.Merchants.Domain.Merchants;

namespace Way2Pay.Merchants.Application.Merchants.CreateMerchant;

public sealed record CreateMerchantResponse(Guid MerchantId, string Name, MerchantStatus Status);
