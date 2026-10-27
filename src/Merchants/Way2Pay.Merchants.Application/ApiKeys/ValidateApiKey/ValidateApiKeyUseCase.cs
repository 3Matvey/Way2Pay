using Way2Pay.Merchants.Application.Merchants;
using Way2Pay.Merchants.Domain.ApiKeys;
using Way2Pay.Merchants.Domain.Merchants;

namespace Way2Pay.Merchants.Application.ApiKeys.ValidateApiKey;

public sealed class ValidateApiKeyUseCase(
    IMerchantApiKeyRepository apiKeyRepository, IMerchantRepository merchantRepository,
    IApiKeySecretProvider secretProvider, TimeProvider timeProvider) : IUseCase
{
    /// <summary>Authenticates an API key and returns its active merchant without modifying stored data.</summary>
    /// <remarks>
    /// Malformed, unknown, expired, revoked and mismatched credentials share one error.
    /// Merchant suspension is disclosed only after verifying the secret and key validity.
    /// Repository failures propagate as technical failures, not invalid credentials.
    /// </remarks>
    public async Task<Result<ValidateApiKeyResponse>> ExecuteAsync(
        ValidateApiKeyRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!ApiKeyFormat.TryParse(request.ApiKey, out var apiKeyId, out var secret))
            return ValidateApiKeyErrors.InvalidApiKey();

        var apiKey = await apiKeyRepository.GetByIdAsync(apiKeyId, cancellationToken);
        if (apiKey is null || !secretProvider.Verify(secret, apiKey.SecretHash))
            return ValidateApiKeyErrors.InvalidApiKey();

        return await ValidateMerchantAsync(apiKey, cancellationToken);
    }

    private async Task<Result<ValidateApiKeyResponse>> ValidateMerchantAsync(
        MerchantApiKey apiKey, CancellationToken cancellationToken)
    {
        var merchant = await merchantRepository.GetByIdAsync(apiKey.MerchantId, cancellationToken);
        if (merchant is null || !apiKey.IsValidAt(timeProvider.GetUtcNow()))
            return ValidateApiKeyErrors.InvalidApiKey();
        if (merchant.Status != MerchantStatus.Active)
            return ValidateApiKeyErrors.MerchantSuspended();
        return new ValidateApiKeyResponse(merchant.Id);
    }
}
