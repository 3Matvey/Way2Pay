using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Way2Pay.CardVault.Authentication;

/// <summary>Documents opaque Bearer credentials only for endpoints using the card-registration policy.</summary>
internal sealed class CardRegistrationOpenApiTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (!context.Description.ActionDescriptor.EndpointMetadata.OfType<IAuthorizeData>()
            .Any(metadata => metadata.Policy == CardRegistrationAuthentication.Policy))
            return Task.CompletedTask;
        AddSecurityScheme(context.Document);
        operation.Security ??= [];
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(CardRegistrationAuthentication.Scheme, context.Document)] = []
        });
        return Task.CompletedTask;
    }

    private static void AddSecurityScheme(OpenApiDocument document)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[CardRegistrationAuthentication.Scheme] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            Description = "Short-lived opaque card-registration token, not a JWT or merchant API key."
        };
    }
}
