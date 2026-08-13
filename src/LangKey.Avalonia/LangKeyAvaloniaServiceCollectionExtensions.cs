using global::Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ArkheideSystem.LangKey.Avalonia;

/// <summary>Registers LangKey parsing and automatic Avalonia localization as one hosted runtime.</summary>
public static class LangKeyAvaloniaServiceCollectionExtensions
{
    /// <summary>
    /// Registers one LangKey document, its resolver, and automatic Avalonia localization for the
    /// specified application. The applicator starts and stops with the containing host.
    /// </summary>
    public static IServiceCollection AddLangKeyAvalonia<TApplication>(
        this IServiceCollection services,
        string path,
        Func<IServiceProvider, string>? initialCulture = null,
        string fallback = "en-US"
    )
        where TApplication : Application
    {
        ArgumentNullException.ThrowIfNull(services);
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("The LangKey.json path cannot be empty.", nameof(path));
        }

        if (string.IsNullOrWhiteSpace(fallback))
        {
            throw new ArgumentException("The fallback culture cannot be empty.", nameof(fallback));
        }

        return AddLangKeyAvaloniaCore<TApplication>(
            services,
            () => services.AddLangKey(path, initialCulture, fallback)
        );
    }

    /// <summary>
    /// Registers Avalonia localization and follows a user-provided, framework-independent culture
    /// source. The source must already be registered as <typeparamref name="TCultureSource" />.
    /// </summary>
    public static IServiceCollection AddLangKeyAvalonia<TApplication, TCultureSource>(
        this IServiceCollection services,
        string path,
        string fallback = "en-US"
    )
        where TApplication : Application
        where TCultureSource : class, ILangKeyCultureSource
    {
        ArgumentNullException.ThrowIfNull(services);
        return AddLangKeyAvaloniaCore<TApplication>(
            services,
            () => services.AddLangKey<TCultureSource>(path, fallback)
        );
    }

    private static IServiceCollection AddLangKeyAvaloniaCore<TApplication>(
        IServiceCollection services,
        Action registerParser
    )
        where TApplication : Application
    {
        EnsureServicesAreAvailable(services);

        registerParser();
        services.AddSingleton<LangKeyAvaloniaApplicator>();
        services.AddSingleton<ILangKeyAvaloniaApplicator>(provider =>
            provider.GetRequiredService<LangKeyAvaloniaApplicator>()
        );
        services.AddSingleton<LangKeyAvaloniaHostedService<TApplication>>();
        services.AddSingleton<IHostedService>(provider =>
            provider.GetRequiredService<LangKeyAvaloniaHostedService<TApplication>>()
        );

        return services;
    }

    private static void EnsureServicesAreAvailable(IServiceCollection services)
    {
        Type[] ownedServiceTypes =
        [
            typeof(ILangKeyAvaloniaApplicator),
            typeof(LangKeyAvaloniaApplicator),
        ];

        var conflict = services.FirstOrDefault(descriptor =>
            ownedServiceTypes.Contains(descriptor.ServiceType)
        );
        if (conflict is not null)
        {
            throw new InvalidOperationException(
                $"Cannot register LangKey Avalonia because service '{conflict.ServiceType.FullName}' is already registered."
            );
        }
    }
}
