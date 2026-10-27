using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Way2Pay.CardVault.CardRegistrationSessions;
using Way2Pay.CardVault.Persistence;

namespace Way2Pay.CardVault.Authentication;

/// <summary>Authenticates an opaque registration token against its stored session verifier.</summary>
/// <remarks>
/// Completed sessions remain valid until expiration so registration can return its saved result.
/// Authentication does not reserve the session; the registration handler must enforce completion and concurrency.
/// </remarks>
internal sealed class CardRegistrationAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder,
    CardVaultDbContext dbContext, TimeProvider clock)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return AuthenticateResult.NoResult();

        var token = ReadToken();
        if (token is null)
            return AuthenticateResult.Fail("Invalid card registration credential.");

        var hash = CardRegistrationToken.ComputeHash(token);
        var session = await dbContext.CardRegistrationSessions.AsNoTracking()
            .SingleOrDefaultAsync(session => session.TokenHash == hash, Context.RequestAborted);

        if (session is null || session.IsExpiredAt(clock.GetUtcNow())
            || session.Status is not (CardRegistrationSessionStatus.Pending or CardRegistrationSessionStatus.Completed))
            return AuthenticateResult.Fail("Invalid card registration credential.");

        return AuthenticateSession(session);
    }

    private string? ReadToken()
    {
        var values = Request.Headers.Authorization;
        if (values.Count != 1 || !AuthenticationHeaderValue.TryParse(values[0], out var header)
            || !string.Equals(header.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase))
            return null;

        var token = header.Parameter;
        return token is { Length: 43 } 
               && token.All(character => char.IsAsciiLetterOrDigit(character) || character is '-' or '_') 
               ? token : null;
    }

    private AuthenticateResult AuthenticateSession(CardRegistrationSession session)
    {
        Context.Features.Set(new CardRegistrationContext(
            session.Id, session.MerchantId, session.ExternalCustomerId));
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, session.Id.ToString())], Scheme.Name);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.Headers.WWWAuthenticate = "Bearer";
        return Task.CompletedTask;
    }
}
