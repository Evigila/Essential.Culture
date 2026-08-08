using System.Globalization;
using Microsoft.Extensions.DependencyInjection;

namespace ArkheideSystem.LangKey;

/// <summary>Registers the framework-independent LangKey runtime with dependency injection.</summary>
public static class LangKeyServiceCollectionExtensions
{
    /// <summary>Registers one shared parser and its read-only resolver surface.</summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="path">
    /// The LangKey JSON path. Relative paths are resolved from <see cref="AppContext.BaseDirectory" />.
    /// </param>
    /// <param name="initialCulture">
    /// An optional factory that obtains the initial culture from another registered service.
    /// </param>
    /// <param name="fallback">The fallback culture required by every translation entry.</param>
    public static IServiceCollection AddLangKey(
        this IServiceCollection services,
        string path,
        Func<IServiceProvider, string>? initialCulture = null,
        string fallback = "en-US"
    )
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

        return AddLangKeyCore(services, path, initialCulture, null, fallback);
    }

    /// <summary>
    /// Registers one shared parser whose culture follows a user-provided source. The source must
    /// already be registered as <typeparamref name="TCultureSource" />.
    /// </summary>
    public static IServiceCollection AddLangKey<TCultureSource>(
        this IServiceCollection services,
        string path,
        string fallback = "en-US"
    )
        where TCultureSource : class, ILangKeyCultureSource
    {
        ArgumentNullException.ThrowIfNull(services);
        return AddLangKeyCore(
            services,
            path,
            null,
            provider => provider.GetRequiredService<TCultureSource>(),
            fallback
        );
    }

    private static IServiceCollection AddLangKeyCore(
        IServiceCollection services,
        string path,
        Func<IServiceProvider, string>? initialCulture,
        Func<IServiceProvider, ILangKeyCultureSource>? cultureSource,
        string fallback
    )
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("The LangKey.json path cannot be empty.", nameof(path));
        }

        if (string.IsNullOrWhiteSpace(fallback))
        {
            throw new ArgumentException("The fallback culture cannot be empty.", nameof(fallback));
        }

        EnsureServicesAreAvailable(services);

        var sourcePath = Path.IsPathFullyQualified(path)
            ? Path.GetFullPath(path)
            : Path.GetFullPath(path, AppContext.BaseDirectory);
        services.AddSingleton(
            new LangKeyRegistration(sourcePath, initialCulture, cultureSource, fallback)
        );
        services.AddSingleton<ILangKeyParser>(provider =>
        {
            var options = provider.GetRequiredService<LangKeyRegistration>();
            if (options.CultureSource is not null)
            {
                return new LangKeyParser(
                    options.SourcePath,
                    options.CultureSource(provider),
                    options.Fallback
                );
            }

            var current = options.InitialCulture?.Invoke(provider);
            if (string.IsNullOrWhiteSpace(current))
            {
                current = string.IsNullOrWhiteSpace(CultureInfo.CurrentUICulture.Name)
                    ? options.Fallback
                    : CultureInfo.CurrentUICulture.Name;
            }

            return new LangKeyParser(options.SourcePath, current, options.Fallback);
        });
        services.AddSingleton<ILangKeyResolver>(provider =>
            provider.GetRequiredService<ILangKeyParser>()
        );

        return services;
    }

    private static void EnsureServicesAreAvailable(IServiceCollection services)
    {
        Type[] ownedServiceTypes =
        [
            typeof(LangKeyRegistration),
            typeof(ILangKeyParser),
            typeof(ILangKeyResolver),
        ];

        var conflict = services.FirstOrDefault(descriptor =>
            ownedServiceTypes.Contains(descriptor.ServiceType)
        );
        if (conflict is not null)
        {
            throw new InvalidOperationException(
                $"Cannot register LangKey because service '{conflict.ServiceType.FullName}' is already registered."
            );
        }
    }

    private sealed record LangKeyRegistration(
        string SourcePath,
        Func<IServiceProvider, string>? InitialCulture,
        Func<IServiceProvider, ILangKeyCultureSource>? CultureSource,
        string Fallback
    );
}
