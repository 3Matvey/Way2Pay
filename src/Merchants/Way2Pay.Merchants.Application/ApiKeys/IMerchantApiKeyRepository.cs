using Way2Pay.Merchants.Domain.ApiKeys;

namespace Way2Pay.Merchants.Application.ApiKeys;

public interface IMerchantApiKeyRepository
{
    /// <summary>Loads a tracked key by its public identifier, or returns null when it does not exist.</summary>
    Task<MerchantApiKey?> GetByIdAsync(Guid apiKeyId, CancellationToken cancellationToken = default);

    /// <summary>Stages the API key without saving; persistence is performed by the unit of work.</summary>
    void Add(MerchantApiKey apiKey);
}
