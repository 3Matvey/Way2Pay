using Way2Pay.Merchants.Application.Common;

namespace Way2Pay.Merchants.Application.ApiKeys.RevokeApiKey;

public sealed class RevokeApiKeyUseCase(
    IMerchantApiKeyRepository apiKeyRepository, IUnitOfWork unitOfWork, TimeProvider timeProvider) : IUseCase
{
    /// <summary>Revokes a merchant-owned API key; repeated revocation succeeds without changing its original timestamp.</summary>
    /// <remarks>The caller must be authorized for the requested merchant. Missing and foreign keys share one error.</remarks>
    public async Task<Result> ExecuteAsync(
        RevokeApiKeyRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var validation = ValidateRequest(request);
        if (!validation.IsSuccess)
            return validation;

        var apiKey = await apiKeyRepository.GetByIdAsync(request.ApiKeyId, cancellationToken);
        if (apiKey is null || apiKey.MerchantId != request.MerchantId)
            return RevokeApiKeyErrors.NotFound();

        apiKey.Revoke(timeProvider.GetUtcNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private static Result ValidateRequest(RevokeApiKeyRequest request)
    {
        if (request.MerchantId == Guid.Empty)
            return RevokeApiKeyErrors.InvalidMerchantId();
        if (request.ApiKeyId == Guid.Empty)
            return RevokeApiKeyErrors.InvalidApiKeyId();
        return Result.Success();
    }
}
