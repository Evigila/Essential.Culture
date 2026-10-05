using System.Collections.ObjectModel;

namespace ArkheideSystem.Essential.Culture.Blazor;

/// <summary>Configures immutable catalogs and scoped initialization at host startup.</summary>
public sealed class LocalizationBuilder
{
    private readonly Dictionary<string, LocalizationCatalog> catalogs = new(StringComparer.Ordinal);
    private readonly List<string> supportedCultures = [];
    private string? defaultCatalog;
    private string defaultCulture = "en-US";
    private string? defaultFormatCulture;
    private Func<IServiceProvider, LocalizationSelection>? initializer;
    private bool completed;

    internal LocalizationBuilder() { }

    /// <summary>Adds a parsed catalog under a unique, case-sensitive identity.</summary>
    public LocalizationBuilder AddCatalog(string catalogId, LocalizationCatalog catalog)
    {
        Check();
        ArgumentException.ThrowIfNullOrWhiteSpace(catalogId);
        ArgumentNullException.ThrowIfNull(catalog);
        if (!catalogs.TryAdd(catalogId, catalog))
            throw new ArgumentException($"Catalog '{catalogId}' is already registered.", nameof(catalogId));
        defaultCatalog ??= catalogId;
        return this;
    }

    /// <summary>Selects the catalog used when no identity is supplied.</summary>
    public LocalizationBuilder SetDefaultCatalog(string catalogId)
    {
        Check();
        ArgumentException.ThrowIfNullOrWhiteSpace(catalogId);
        defaultCatalog = catalogId;
        return this;
    }

    /// <summary>Sets the fallback selection when ambient cultures aren't available or supported.</summary>
    public LocalizationBuilder SetDefaultCulture(string culture, string? formatCulture = null)
    {
        Check();
        defaultCulture = Normalize(culture);
        defaultFormatCulture = formatCulture is null ? null : Normalize(formatCulture);
        return this;
    }

    /// <summary>Restricts UI selections. When omitted, the default catalog's cultures are allowed.</summary>
    public LocalizationBuilder AddSupportedCultures(params string[] cultures)
    {
        Check();
        ArgumentNullException.ThrowIfNull(cultures);
        foreach (var culture in cultures)
        {
            var normalized = Normalize(culture);
            if (!supportedCultures.Contains(normalized, StringComparer.OrdinalIgnoreCase))
                supportedCultures.Add(normalized);
        }
        return this;
    }

    /// <summary>Initializes each scope from host-owned state before its first localized render.</summary>
    public LocalizationBuilder InitializeWith(Func<IServiceProvider, LocalizationSelection> initialize)
    {
        Check();
        ArgumentNullException.ThrowIfNull(initialize);
        initializer = initialize;
        return this;
    }

    internal LocalizationOptions Complete()
    {
        Check();
        if (defaultCatalog is null || !catalogs.ContainsKey(defaultCatalog))
            throw new InvalidOperationException("A registered default catalog is required.");
        var cultures = supportedCultures.Count == 0
            ? catalogs[defaultCatalog].AvailableCultures.Select(Normalize).ToArray()
            : supportedCultures.ToArray();
        if (!cultures.Contains(defaultCulture, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("The default culture must be in the supported UI cultures.");
        completed = true;
        return new LocalizationOptions(
            new ReadOnlyDictionary<string, LocalizationCatalog>(new Dictionary<string, LocalizationCatalog>(catalogs, StringComparer.Ordinal)),
            defaultCatalog, defaultCulture, defaultFormatCulture ?? defaultCulture,
            Array.AsReadOnly(cultures), initializer);
    }

    internal static string Normalize(string culture) => CultureName.Normalize(culture);

    private void Check()
    {
        if (completed) throw new InvalidOperationException("Localization configuration is already complete.");
    }
}

internal sealed record LocalizationOptions(
    IReadOnlyDictionary<string, LocalizationCatalog> Catalogs,
    string DefaultCatalog,
    string DefaultCulture,
    string DefaultFormatCulture,
    IReadOnlyList<string> SupportedCultures,
    Func<IServiceProvider, LocalizationSelection>? Initialize);
