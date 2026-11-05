using Way2Pay.Gateway.Authentication;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace Way2Pay.Gateway.Proxy;

/// <summary>Removes client-supplied merchant identity and forwards only authenticated merchant context.</summary>
internal static class MerchantContextTransform
{
    public static void Configure(TransformBuilderContext context)
    {
        context.AddRequestHeaderRemove(MerchantApiKeyAuthentication.MerchantIdHeader);
        if (string.Equals(context.Route.AuthorizationPolicy, MerchantApiKeyAuthentication.Policy,
            StringComparison.OrdinalIgnoreCase))
            context.AddRequestTransform(ForwardMerchantAsync);
    }

    private static ValueTask ForwardMerchantAsync(RequestTransformContext context)
    {
        var identity = context.HttpContext.User.Identities.SingleOrDefault(identity =>
            identity.IsAuthenticated && identity.AuthenticationType == MerchantApiKeyAuthentication.Scheme);

        var merchantId = identity?.FindFirst(MerchantApiKeyAuthentication.MerchantIdClaim)?.Value
            ?? throw new InvalidOperationException("The authenticated merchant context is missing.");

        context.ProxyRequest.Headers.Remove("Authorization");
        context.ProxyRequest.Headers.Add(MerchantApiKeyAuthentication.MerchantIdHeader, merchantId);
        return ValueTask.CompletedTask;
    }
}
