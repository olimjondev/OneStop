using Microsoft.Extensions.DependencyInjection;
using OneStop.Application.Interfaces;
using OneStop.Infrastructure.Persistence.InMemory;

namespace OneStop.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Infrastructure layer services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Register in-memory repositories
        services.AddSingleton<IProductRepository, InMemoryProductRepository>();
        services.AddSingleton<IDiscountPromotionRepository, InMemoryDiscountPromotionRepository>();
        services.AddSingleton<IPointsPromotionRepository, InMemoryPointsPromotionRepository>();

        return services;
    }
}
