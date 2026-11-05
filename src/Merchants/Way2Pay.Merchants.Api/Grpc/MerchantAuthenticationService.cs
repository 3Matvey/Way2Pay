using Way2Pay.Merchants.Contracts.Protos.Authentication.V1;
using Grpc.Core;
using Way2Pay.Common.Results;
using Way2Pay.Merchants.Application.ApiKeys.ValidateApiKey;
using ApplicationRequest = Way2Pay.Merchants.Application.ApiKeys.ValidateApiKey.ValidateApiKeyRequest;
using GrpcRequest = Way2Pay.Merchants.Contracts.Protos.Authentication.V1.ValidateApiKeyRequest;
using GrpcResponse = Way2Pay.Merchants.Contracts.Protos.Authentication.V1.ValidateApiKeyResponse;

namespace Way2Pay.Merchants.Api.Grpc;

/// <summary>Exposes merchant API key validation to internal gRPC callers.</summary>
public sealed class MerchantAuthenticationService(ValidateApiKeyUseCase validateApiKey)
    : MerchantAuthentication.MerchantAuthenticationBase
{
    /// <summary>Returns the active merchant identity or an authentication/permission error.</summary>
    public override async Task<GrpcResponse> ValidateApiKey(GrpcRequest request, ServerCallContext context)
    {
        var result = await validateApiKey.ExecuteAsync(
            new ApplicationRequest(request.ApiKey), context.CancellationToken);

        if (!result.IsSuccess)
            throw MapError(result.Error);

        return new GrpcResponse { MerchantId = result.Value.MerchantId.ToString() };
    }

    private static RpcException MapError(Error error) => error.ErrorType switch
    {
        ErrorType.AccessUnauthorized => new(new Status(StatusCode.Unauthenticated, error.Description)),
        ErrorType.AccessForbidden => new(new Status(StatusCode.PermissionDenied, error.Description)),
        _ => new(new Status(StatusCode.Internal, "API key validation failed."))
    };
}
