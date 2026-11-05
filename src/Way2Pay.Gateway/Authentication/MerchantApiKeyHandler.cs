using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Grpc.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Way2Pay.Merchants.Contracts.Protos.Authentication.V1;

namespace Way2Pay.Gateway.Authentication;

/// <summary>Validates merchant credentials through Merchants only on routes using this scheme.</summary>
internal sealed class MerchantApiKeyHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    MerchantAuthentication.MerchantAuthenticationClient client)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    private int _failureStatusCode = StatusCodes.Status401Unauthorized;

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!IsMerchantRoute() || !Request.Headers.ContainsKey("Authorization"))
            return AuthenticateResult.NoResult();

        var apiKey = ReadApiKey();
        if (apiKey is null)
            return AuthenticateResult.Fail("Invalid merchant credential.");

        return await ValidateAsync(apiKey);
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = _failureStatusCode;
        if (_failureStatusCode == StatusCodes.Status401Unauthorized)
            Response.Headers.WWWAuthenticate = "Bearer";

        return Task.CompletedTask;
    }

    // A sole scheme may become ASP.NET Core's automatic default; public routes must still skip validation.
    private bool IsMerchantRoute() => Context.GetEndpoint()?.Metadata
        .GetOrderedMetadata<IAuthorizeData>().Any(data => string.Equals(
            data.Policy, MerchantApiKeyAuthentication.Policy, StringComparison.OrdinalIgnoreCase)) == true;

    private string? ReadApiKey()
    {
        var values = Request.Headers.Authorization;
        if (values.Count != 1 || 
            !AuthenticationHeaderValue.TryParse(values[0], out var header) || 
            !string.Equals(header.Scheme,"Bearer", StringComparison.OrdinalIgnoreCase) || 
            string.IsNullOrWhiteSpace(header.Parameter))
        {
            return null;
        }

        return header.Parameter;
    }

    private async Task<AuthenticateResult> ValidateAsync(string apiKey)
    {
        try
        {
            var response = await client.ValidateApiKeyAsync(
                request: new ValidateApiKeyRequest { ApiKey = apiKey },
                deadline: DateTime.UtcNow.AddSeconds(3),
                cancellationToken: Context.RequestAborted);

            return AuthenticateMerchant(response.MerchantId);
        }
        catch (RpcException exception)
        {
            Context.RequestAborted.ThrowIfCancellationRequested();
            return FailWithStatus(MapStatusCode(exception.StatusCode));
        }
        catch (HttpRequestException)
        {
            Context.RequestAborted.ThrowIfCancellationRequested();
            return FailWithStatus(StatusCodes.Status503ServiceUnavailable);
        }

        static int MapStatusCode(StatusCode statusCode) => statusCode switch
        {
            StatusCode.Unauthenticated => StatusCodes.Status401Unauthorized,
            StatusCode.PermissionDenied => StatusCodes.Status403Forbidden,
            StatusCode.Unavailable or StatusCode.DeadlineExceeded => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status502BadGateway
        };
    }

    private AuthenticateResult AuthenticateMerchant(string value)
    {
        if (!Guid.TryParse(value, out var merchantId) || merchantId == Guid.Empty)
            return FailWithStatus(StatusCodes.Status502BadGateway);

        var identity = new ClaimsIdentity(
            claims: [new Claim(MerchantApiKeyAuthentication.MerchantIdClaim, merchantId.ToString())], 
            authenticationType: Scheme.Name);

        return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name));
    }

    private AuthenticateResult FailWithStatus(int statusCode)
    {
        _failureStatusCode = statusCode;
        return AuthenticateResult.Fail("Merchant authentication did not succeed.");
    }
}
