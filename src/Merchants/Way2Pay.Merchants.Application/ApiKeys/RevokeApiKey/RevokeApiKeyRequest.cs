namespace Way2Pay.Merchants.Application.ApiKeys.RevokeApiKey;

public sealed record RevokeApiKeyRequest(Guid MerchantId, Guid ApiKeyId);
