using Way2Pay.Merchants.Application.Common;
using Way2Pay.Merchants.Application.Merchants;
using Way2Pay.Merchants.Domain.ApiKeys;

namespace Way2Pay.Merchants.Application.ApiKeys.IssueApiKey;

public sealed class IssueApiKeyUseCase(
    IMerchantRepository merchantRepository, IMerchantApiKeyRepository apiKeyRepository,
    IApiKeySecretProvider secretProvider, IUnitOfWork unitOfWork, TimeProvider timeProvider) : IUseCase
{
    /// <summary>Issues a key for an existing merchant, including a suspended merchant preparing replacement credentials.</summary>
    /// <remarks>
    /// Merchant suspension is enforced during authentication, not issuance. Only the hash is saved.
    /// Each successful invocation issues a new key; a lost response cannot be replayed to recover its secret.
    /// </remarks>
    public async Task<Result<IssueApiKeyResponse>> ExecuteAsync(
        IssueApiKeyRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var validation = ValidateRequest(request);
        if (!validation.IsSuccess)
            return validation.Error;

        var merchant = await merchantRepository.GetByIdAsync(request.MerchantId, cancellationToken);
        if (merchant is null)
            return IssueApiKeyErrors.MerchantNotFound();

        return await IssueAsync(request, cancellationToken);
    }

    private static Result ValidateRequest(IssueApiKeyRequest request)
    {
        if (request.MerchantId == Guid.Empty)
            return IssueApiKeyErrors.InvalidMerchantId();
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 200)
            return IssueApiKeyErrors.InvalidName();
        return Result.Success();
    }

    private async Task<Result<IssueApiKeyResponse>> IssueAsync(
        IssueApiKeyRequest request, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        if (request.ExpiresAt.HasValue && request.ExpiresAt.Value <= now)
            return IssueApiKeyErrors.InvalidExpiration();

        var generated = secretProvider.Generate();
        var apiKey = new MerchantApiKey(request.MerchantId, request.Name, generated.Hash, now, request.ExpiresAt);
        apiKeyRepository.Add(apiKey);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new IssueApiKeyResponse(apiKey.Id, ApiKeyFormat.Create(apiKey.Id, generated.Secret), apiKey.ExpiresAt);
    }
}
