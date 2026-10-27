namespace Way2Pay.Merchants.Application.ApiKeys.IssueApiKey;

public sealed record IssueApiKeyRequest(Guid MerchantId, string Name, DateTimeOffset? ExpiresAt = null);
