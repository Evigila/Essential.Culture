using System.Collections.Frozen;
using System.Globalization;

namespace ArkheideSystem.Essential.Culture.Blazor;

internal sealed class LocalizationSession : ILocalizationService
{
    private readonly LocalizationOptions options;
    private readonly Lock gate = new();
    private SessionState state;

    internal LocalizationSession(LocalizationOptions options, IServiceProvider services)
    {
        this.options = options;
        var selection = options.Initialize is { } initialize
            ? initialize(services) ?? throw new InvalidOperationException("The localization initializer returned null.")
            : AmbientSelection(options);
        state = CreateState(selection.Culture, selection.FormatCulture);
    }

    public string Culture => Volatile.Read(ref state).Culture;
    public CultureInfo FormatCulture => Volatile.Read(ref state).FormatCulture;
    public IReadOnlyList<string> AvailableCultures => options.SupportedCultures;
    public event EventHandler? Changed;

    public void SetCulture(string culture, string? formatCulture = null)
    {
        var normalized = LocalizationBuilder.Normalize(culture);
        var normalizedFormat = formatCulture is null ? normalized : LocalizationBuilder.Normalize(formatCulture);
        lock (gate)
        {
            if (state.Culture == normalized && state.FormatCultureName == normalizedFormat) return;
            var next = CreateState(normalized, normalizedFormat);
            Volatile.Write(ref state, next);
        }
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public string Parse(string token, params object?[] arguments) =>
        ParseFrom(options.DefaultCatalog, token, arguments);

    public string ParseFrom(string catalogId, string token, params object?[] arguments) =>
        GetContext(catalogId).Parse(token, arguments);

    public bool TryParse(string token, out string value) => TryParseFrom(options.DefaultCatalog, token, out value);
    public bool TryParse(string token, object?[] arguments, out string value) =>
        TryParseFrom(options.DefaultCatalog, token, arguments, out value);
    public bool TryParseFrom(string catalogId, string token, out string value) => GetContext(catalogId).TryParse(token, out value);
    public bool TryParseFrom(string catalogId, string token, object?[] arguments, out string value) =>
        GetContext(catalogId).TryParse(token, arguments, out value);
    public bool Contains(string token) => ContainsFrom(options.DefaultCatalog, token);
    public bool ContainsFrom(string catalogId, string token) => GetContext(catalogId).Contains(token);

    private LocalizationContext GetContext(string catalogId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(catalogId);
        var snapshot = Volatile.Read(ref state);
        return snapshot.Contexts.TryGetValue(catalogId, out var context)
            ? context : throw new KeyNotFoundException($"Catalog '{catalogId}' is not registered.");
    }

    private SessionState CreateState(string culture, string? formatCulture)
    {
        var normalized = LocalizationBuilder.Normalize(culture);
        if (!options.SupportedCultures.Contains(normalized, StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException($"UI culture '{normalized}' isn't supported by this host.", nameof(culture));
        var normalizedFormat = formatCulture is null ? normalized : LocalizationBuilder.Normalize(formatCulture);
        var contexts = options.Catalogs.ToFrozenDictionary(
            pair => pair.Key,
            pair => new LocalizationContext(pair.Value, normalized, normalizedFormat),
            StringComparer.Ordinal);
        return new SessionState(normalized, normalizedFormat, contexts[options.DefaultCatalog].FormatCulture, contexts);
    }

    private static LocalizationSelection AmbientSelection(LocalizationOptions options)
    {
        var ui = CultureInfo.CurrentUICulture.Name;
        if (!options.SupportedCultures.Contains(ui, StringComparer.OrdinalIgnoreCase)) ui = options.DefaultCulture;
        var format = CultureInfo.CurrentCulture.Name;
        if (string.IsNullOrEmpty(format)) format = options.DefaultFormatCulture;
        return new LocalizationSelection(ui, format);
    }

    private sealed record SessionState(string Culture, string FormatCultureName, CultureInfo FormatCulture,
        FrozenDictionary<string, LocalizationContext> Contexts);
}
