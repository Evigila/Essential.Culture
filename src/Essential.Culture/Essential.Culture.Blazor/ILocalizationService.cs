using System.Globalization;

namespace ArkheideSystem.Essential.Culture.Blazor;

/// <summary>Resolves text with a language selection belonging to the current request or circuit.</summary>
public interface ILocalizationService
{
    /// <summary>Gets the selected UI culture.</summary>
    string Culture { get; }

    /// <summary>Gets the read-only culture used for composite formatting.</summary>
    CultureInfo FormatCulture { get; }

    /// <summary>Gets the UI cultures allowed by the host.</summary>
    IReadOnlyList<string> AvailableCultures { get; }

    /// <summary>Occurs after this scope's selection changes.</summary>
    event EventHandler? Changed;

    /// <summary>Changes this scope only. Omitting formatCulture uses the UI culture for formatting.</summary>
    void SetCulture(string culture, string? formatCulture = null);

    /// <summary>Resolves a token in the default catalog and optionally formats it.</summary>
    string Parse(string token, params object?[] arguments);

    /// <summary>Resolves a token in an explicitly identified catalog and optionally formats it.</summary>
    string ParseFrom(string catalogId, string token, params object?[] arguments);

    /// <summary>Attempts to resolve a token in the default catalog.</summary>
    bool TryParse(string token, out string value);

    /// <summary>Attempts to resolve and format a token in the default catalog.</summary>
    bool TryParse(string token, object?[] arguments, out string value);

    /// <summary>Attempts to resolve a token in an explicitly identified catalog.</summary>
    bool TryParseFrom(string catalogId, string token, out string value);

    /// <summary>Attempts to resolve and format a token in an explicitly identified catalog.</summary>
    bool TryParseFrom(string catalogId, string token, object?[] arguments, out string value);

    /// <summary>Tests whether a token exists in the default catalog.</summary>
    bool Contains(string token);

    /// <summary>Tests whether a token exists in an explicitly identified catalog.</summary>
    bool ContainsFrom(string catalogId, string token);
}

/// <summary>Describes initial UI and formatting cultures without HTTP or browser dependencies.</summary>
/// <param name="Culture">The UI culture.</param>
/// <param name="FormatCulture">The formatting culture, or null to use the UI culture.</param>
public sealed record LocalizationSelection(string Culture, string? FormatCulture = null);
