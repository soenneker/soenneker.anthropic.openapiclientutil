using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Anthropic.HttpClients.Registrars;
using Soenneker.Anthropic.OpenApiClientUtil.Abstract;

namespace Soenneker.Anthropic.OpenApiClientUtil.Registrars;

/// <summary>
/// Registers the OpenAPI client utility for dependency injection.
/// </summary>
public static class AnthropicOpenApiClientUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="AnthropicOpenApiClientUtil"/> as a singleton service. <para/>
    /// </summary>
    public static IServiceCollection AddAnthropicOpenApiClientUtilAsSingleton(this IServiceCollection services)
    {
        services.AddAnthropicOpenApiHttpClientAsSingleton()
                .TryAddSingleton<IAnthropicOpenApiClientUtil, AnthropicOpenApiClientUtil>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="AnthropicOpenApiClientUtil"/> as a scoped service. <para/>
    /// </summary>
    public static IServiceCollection AddAnthropicOpenApiClientUtilAsScoped(this IServiceCollection services)
    {
        services.AddAnthropicOpenApiHttpClientAsSingleton()
                .TryAddScoped<IAnthropicOpenApiClientUtil, AnthropicOpenApiClientUtil>();

        return services;
    }
}
