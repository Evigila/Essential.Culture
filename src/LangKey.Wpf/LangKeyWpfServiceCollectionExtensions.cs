using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ArkheideSystem.LangKey.Wpf;

/// <summary>Registers LangKey parsing and automatic WPF localization as one hosted runtime.</summary>
public static class LangKeyWpfServiceCollectionExtensions
{
    /// <summary>
    /// Registers one LangKey document, its resolver, and automatic WPF localization for the
    /// specified application. The applicator starts and stops with the containing host.
    /// </summary>
    /// <typeparam name="TApplication">The WPF application registered in the service provider.</typeparam>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="path">
    /// The LangKey JSON path. Relative paths are resolved from <see cref="AppContext.BaseDirectory" />.
    /// </param>
    /// <param name="initialCulture">
    /// An optional factory that obtains the initial culture from another registered service.
    /// </param>
    /// <param name="fallback">The fallback culture required by every translation entry.</param>
    /// <returns>The same service collection for chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when LangKey WPF services have already been registered or conflict with existing
    /// registrations owned by this method.
    /// </exception>
    public static IServiceCollection AddLangKeyWpf<TApplication>(
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

        return AddLangKeyWpfCore<TApplication>(
            services,
            () => services.AddLangKey(path, initialCulture, fallback)
        );
    }

    /// <summary>
    /// Registers WPF localization and follows a user-provided, framework-independent culture
    /// source. The source must already be registered as <typeparamref name="TCultureSource" />.
    /// </summary>
    public static IServiceCollection AddLangKeyWpf<TApplication, TCultureSource>(
        this IServiceCollection services,
        string path,
        string fallback = "en-US"
    )
        where TApplication : Application
        where TCultureSource : class, ILangKeyCultureSource
    {
        ArgumentNullException.ThrowIfNull(services);
        return AddLangKeyWpfCore<TApplication>(
            services,
            () => services.AddLangKey<TCultureSource>(path, fallback)
        );
    }

    private static IServiceCollection AddLangKeyWpfCore<TApplication>(
        IServiceCollection services,
        Action registerParser
    )
        where TApplication : Application
    {
        EnsureServicesAreAvailable(services);

        registerParser();
        services.AddSingleton<LangKeyWpfApplicator>();
        services.AddSingleton<ILangKeyWpfApplicator>(provider =>
            provider.GetRequiredService<LangKeyWpfApplicator>()
        );
        services.AddSingleton<LangKeyWpfHostedService<TApplication>>();
        services.AddSingleton<IHostedService>(provider =>
            provider.GetRequiredService<LangKeyWpfHostedService<TApplication>>()
        );

        return services;
    }

    private static void EnsureServicesAreAvailable(IServiceCollection services)
    {
        Type[] ownedServiceTypes =
        [
            typeof(ILangKeyWpfApplicator),
            typeof(LangKeyWpfApplicator),
        ];

        var conflict = services.FirstOrDefault(descriptor =>
            ownedServiceTypes.Contains(descriptor.ServiceType)
        );
        if (conflict is not null)
        {
            throw new InvalidOperationException(
                $"Cannot register LangKey WPF because service '{conflict.ServiceType.FullName}' is already registered."
            );
        }
    }

}
