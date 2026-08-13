using ArkheideSystem.LangKey;
using Microsoft.Extensions.DependencyInjection;

namespace ArkheideSystem.LangKey.WinUI;

/// <summary>Registers LangKey parsing and automatic WinUI localization.</summary>
public static class LangKeyWinUIServiceCollectionExtensions
{
    /// <summary>Registers one LangKey document, its resolver, and the WinUI applicator.</summary>
    public static IServiceCollection AddLangKeyWinUI(
        this IServiceCollection services,
        string path,
        Func<IServiceProvider, string>? initialCulture = null,
        string fallback = "en-US"
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        EnsureServicesAreAvailable(services);
        services.AddLangKey(path, initialCulture, fallback);
        return AddApplicator(services);
    }

    /// <summary>
    /// Registers WinUI localization and follows a user-provided, framework-independent culture
    /// source. The source must already be registered as <typeparamref name="TCultureSource" />.
    /// </summary>
    public static IServiceCollection AddLangKeyWinUI<TCultureSource>(
        this IServiceCollection services,
        string path,
        string fallback = "en-US"
    )
        where TCultureSource : class, ILangKeyCultureSource
    {
        ArgumentNullException.ThrowIfNull(services);
        EnsureServicesAreAvailable(services);
        services.AddLangKey<TCultureSource>(path, fallback);
        return AddApplicator(services);
    }

    private static IServiceCollection AddApplicator(IServiceCollection services)
    {
        services.AddSingleton<LangKeyWinUIApplicator>();
        services.AddSingleton<ILangKeyWinUIApplicator>(provider =>
            provider.GetRequiredService<LangKeyWinUIApplicator>()
        );
        return services;
    }

    private static void EnsureServicesAreAvailable(IServiceCollection services)
    {
        Type[] ownedServiceTypes =
        [
            typeof(ILangKeyWinUIApplicator),
            typeof(LangKeyWinUIApplicator),
        ];

        var conflict = services.FirstOrDefault(descriptor =>
            ownedServiceTypes.Contains(descriptor.ServiceType)
        );
        if (conflict is not null)
        {
            throw new InvalidOperationException(
                $"Cannot register LangKey WinUI because service '{conflict.ServiceType.FullName}' is already registered."
            );
        }
    }
}
