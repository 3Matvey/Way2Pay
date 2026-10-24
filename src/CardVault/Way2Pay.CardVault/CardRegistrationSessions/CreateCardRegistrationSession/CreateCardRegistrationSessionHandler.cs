using Microsoft.Extensions.Options;
using Way2Pay.CardVault.Persistence;
using Way2Pay.Common.Results;

namespace Way2Pay.CardVault.CardRegistrationSessions.CreateCardRegistrationSession;

internal sealed class CreateCardRegistrationSessionHandler(
    CardVaultDbContext dbContext, TimeProvider timeProvider, IOptions<CardRegistrationSessionOptions> options)
{
    /// <summary>Persists a pending registration and returns its secret token only after saving succeeds.</summary>
    /// <remarks>
    /// MerchantId must come from a verified merchant integration, never from an untrusted request body.
    /// This handler does not authenticate the caller. The database stores only the token hash.
    /// </remarks>
    public async Task<Result<CreateCardRegistrationSessionResponse>> ExecuteAsync(
        CreateCardRegistrationSessionRequest request, Guid merchantId, CancellationToken cancellationToken = default)
    {
        var validation = Validate(request, merchantId);
        if (!validation.IsSuccess)
            return validation.Error;

        var now = timeProvider.GetUtcNow();
        var token = CardRegistrationToken.Generate();
        var session = new CardRegistrationSession(merchantId, CardRegistrationToken.ComputeHash(token),
            now, now.Add(options.Value.Lifetime), request.ExternalCustomerId);

        dbContext.CardRegistrationSessions.Add(session);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateCardRegistrationSessionResponse(session.Id, token, session.ExpiresAt);
    }

    private static Result Validate(CreateCardRegistrationSessionRequest request, Guid merchantId)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (merchantId == Guid.Empty)
            return CreateCardRegistrationSessionErrors.InvalidMerchantId();
        if (request.ExternalCustomerId is not null
            && (string.IsNullOrWhiteSpace(request.ExternalCustomerId) || request.ExternalCustomerId.Length > 450))
            return CreateCardRegistrationSessionErrors.InvalidExternalCustomerId();

        return Result.Success();
    }
}
