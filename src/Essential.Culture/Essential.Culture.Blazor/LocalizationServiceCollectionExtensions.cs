using ArkheideSystem.Essential.Culture.Blazor;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Registers optional Blazor localization without using the static desktop facade.</summary>
public static class LocalizationServiceCollectionExtensions
{
    /// <summary>Registers immutable catalogs and a localization service per HTTP request or circuit.</summary>
    public static IServiceCollection AddCultureBlazor(this IServiceCollection services,
        Action<LocalizationBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);
        if (services.Any(service => service.ServiceType == typeof(ILocalizationService)))
            throw new InvalidOperationException("Blazor localization is already registered.");
        var builder = new LocalizationBuilder();
        configure(builder);
        var options = builder.Complete();
        services.AddSingleton(options);
        services.AddScoped<ILocalizationService>(provider => new LocalizationSession(options, provider));
        return services;
    }
}
