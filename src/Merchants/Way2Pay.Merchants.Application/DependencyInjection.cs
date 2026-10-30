using Microsoft.Extensions.DependencyInjection;

namespace Way2Pay.Merchants.Application;

public static class DependencyInjection
{
    /// <summary>Registers concrete use cases from this assembly with a scoped lifetime.</summary>
    public static IServiceCollection AddMerchantsApplication(this IServiceCollection services)
    {
        var useCases = typeof(DependencyInjection).Assembly
                .GetTypes()
                .Where(x => x is { IsClass: true, IsAbstract: false })
                .Where(x => typeof(IUseCase).IsAssignableFrom(x));

        foreach (var useCase in useCases)
            services.AddScoped(useCase);

        return services;
    }
}
