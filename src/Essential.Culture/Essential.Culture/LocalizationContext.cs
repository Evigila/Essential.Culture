using System.Collections.Frozen;
using System.Globalization;
using Translation = ArkheideSystem.Essential.Culture.LocalizationCatalog.Translation;

namespace ArkheideSystem.Essential.Culture;

/// <summary>Resolves shared catalog data using an independent culture and formatting selection.</summary>
public sealed class LocalizationContext
{
    private readonly Lock stateGate = new();
    private readonly LocalizationCatalog catalog;
    private RuntimeState state;

    /// <summary>
    /// Creates an independent context without file I/O or ambient culture changes. A null formatting
    /// culture follows the selected translation culture, matching the desktop compatibility facade.
    /// </summary>
    public LocalizationContext(
        LocalizationCatalog catalog,
        string culture = "en-US",
        string? formatCulture = null
    )
    {
        ArgumentNullException.ThrowIfNull(catalog);
        this.catalog = catalog;
        var normalized = KeyValidation.NormalizeCulture(culture, nameof(culture));
        var normalizedFormat = formatCulture is null
            ? normalized
            : KeyValidation.NormalizeCulture(formatCulture, nameof(formatCulture));
        state = CreateState(normalized, normalizedFormat);
    }

    /// <summary>Gets the selected translation culture name.</summary>
    public string Culture => Volatile.Read(ref state).Culture;

    /// <summary>Gets selectable cultures, or declared cultures when the catalog has no policy.</summary>
    public IReadOnlyList<string> AvailableCultures => catalog.AvailableCultures;

    /// <summary>Gets all normalized cultures authored in the shared catalog's documents.</summary>
    public IReadOnlyList<string> DeclaredCultures => catalog.DeclaredCultures;

    /// <summary>Gets the read-only effective formatting culture, independent of ambient culture.</summary>
    public CultureInfo FormatCulture => Volatile.Read(ref state).FormatCulture;

    /// <summary>Raised synchronously after this context's selected culture pair changes.</summary>
    public event EventHandler? Changed;

    /// <summary>
    /// Atomically changes translation and formatting cultures. A null formatCulture follows culture.
    /// Equivalent normalized pairs do not raise Changed.
    /// </summary>
    public void SetCulture(string culture, string? formatCulture = null)
    {
        var normalized = KeyValidation.NormalizeCulture(culture, nameof(culture));
        var normalizedFormat = formatCulture is null
            ? normalized
            : KeyValidation.NormalizeCulture(formatCulture, nameof(formatCulture));
        lock (stateGate)
        {
            if (
                string.Equals(state.Culture, normalized, StringComparison.Ordinal)
                && string.Equals(state.FormatCultureName, normalizedFormat, StringComparison.Ordinal)
            )
            {
                return;
            }

            var next = string.Equals(state.Culture, normalized, StringComparison.Ordinal)
                ? state with
                {
                    FormatCultureName = normalizedFormat,
                    FormatCulture = GetFormatCulture(normalizedFormat),
                }
                : CreateState(normalized, normalizedFormat);
            Volatile.Write(ref state, next);
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Returns whether a raw key or stable token is declared.</summary>
    public bool Contains(string token) => TryResolve(Volatile.Read(ref state), token, out _);

    /// <summary>Attempts lookup using this context. Formatting errors are not suppressed.</summary>
    public bool TryParse(string token, out string value)
    {
        if (TryResolve(Volatile.Read(ref state), token, out var resolved))
        {
            value = resolved.Text;
            return true;
        }

        value = string.Empty;
        return false;
    }

    /// <summary>Attempts lookup using this context. Formatting errors are not suppressed.</summary>
    public bool TryParse(string token, object?[] arguments, out string value)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        var snapshot = Volatile.Read(ref state);
        if (!TryResolve(snapshot, token, out var resolved))
        {
            value = string.Empty;
            return false;
        }

        value = arguments.Length == 0
            ? resolved.Text
            : string.Format(snapshot.FormatCulture, resolved.Format, arguments);
        return true;
    }

    /// <summary>Attempts lookup using this context. Formatting errors are not suppressed.</summary>
    public bool TryParse<TArg0>(string token, TArg0 argument0, out string value)
    {
        var snapshot = Volatile.Read(ref state);
        if (!TryResolve(snapshot, token, out var resolved))
        {
            value = string.Empty;
            return false;
        }

        value = string.Format(snapshot.FormatCulture, resolved.Format, argument0);
        return true;
    }

    /// <summary>Attempts lookup using this context. Formatting errors are not suppressed.</summary>
    public bool TryParse<TArg0, TArg1>(
        string token,
        TArg0 argument0,
        TArg1 argument1,
        out string value
    )
    {
        var snapshot = Volatile.Read(ref state);
        if (!TryResolve(snapshot, token, out var resolved))
        {
            value = string.Empty;
            return false;
        }

        value = string.Format(snapshot.FormatCulture, resolved.Format, argument0, argument1);
        return true;
    }

    /// <summary>Attempts lookup using this context. Formatting errors are not suppressed.</summary>
    public bool TryParse<TArg0, TArg1, TArg2>(
        string token,
        TArg0 argument0,
        TArg1 argument1,
        TArg2 argument2,
        out string value
    )
    {
        var snapshot = Volatile.Read(ref state);
        if (!TryResolve(snapshot, token, out var resolved))
        {
            value = string.Empty;
            return false;
        }

        value = string.Format(
            snapshot.FormatCulture,
            resolved.Format,
            argument0,
            argument1,
            argument2
        );
        return true;
    }

    /// <summary>Resolves a raw key or stable token using this context and optional format arguments.</summary>
    public string Parse(string token)
    {
        var resolved = ResolveOrFallback(Volatile.Read(ref state), token);
        return resolved?.Text ?? token;
    }

    /// <summary>Resolves a raw key or stable token using this context and optional format arguments.</summary>
    public string Parse(string token, params object?[] arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        var snapshot = Volatile.Read(ref state);
        var resolved = ResolveOrFallback(snapshot, token);
        if (resolved is null)
        {
            return token;
        }

        return arguments.Length == 0
            ? resolved.Text
            : string.Format(snapshot.FormatCulture, resolved.Format, arguments);
    }

    /// <summary>Resolves a raw key or stable token using this context and optional format arguments.</summary>
    public string Parse<TArg0>(string token, TArg0 argument0)
    {
        var snapshot = Volatile.Read(ref state);
        var resolved = ResolveOrFallback(snapshot, token);
        return resolved is null
            ? token
            : string.Format(snapshot.FormatCulture, resolved.Format, argument0);
    }

    /// <summary>Resolves a raw key or stable token using this context and optional format arguments.</summary>
    public string Parse<TArg0, TArg1>(string token, TArg0 argument0, TArg1 argument1)
    {
        var snapshot = Volatile.Read(ref state);
        var resolved = ResolveOrFallback(snapshot, token);
        return resolved is null
            ? token
            : string.Format(snapshot.FormatCulture, resolved.Format, argument0, argument1);
    }

    /// <summary>Resolves a raw key or stable token using this context and optional format arguments.</summary>
    public string Parse<TArg0, TArg1, TArg2>(
        string token,
        TArg0 argument0,
        TArg1 argument1,
        TArg2 argument2
    )
    {
        var snapshot = Volatile.Read(ref state);
        var resolved = ResolveOrFallback(snapshot, token);
        return resolved is null
            ? token
            : string.Format(
                snapshot.FormatCulture,
                resolved.Format,
                argument0,
                argument1,
                argument2
            );
    }

    private static Translation? ResolveOrFallback(RuntimeState snapshot, string token)
    {
        if (TryResolve(snapshot, token, out var resolved))
        {
            return resolved;
        }

        if (!KeyToken.TryGetKey(token, out _))
        {
            throw new ArgumentException("Value invalid.", nameof(token));
        }

        return null;
    }

    private static bool TryResolve(
        RuntimeState snapshot,
        string? token,
        out Translation translation
    )
    {
        if (token is not null)
        {
            // Generated tokens take this branch and are looked up verbatim: no substring and no regex.
            var values = token.StartsWith(KeyToken.Prefix, StringComparison.Ordinal)
                ? snapshot.TokenValues
                : snapshot.RawValues;
            if (values.TryGetValue(token, out translation!))
            {
                return true;
            }
        }

        translation = null!;
        return false;
    }

    private static CultureInfo GetFormatCulture(string culture)
    {
        try
        {
            var formatCulture = CultureInfo.GetCultureInfo(culture);
            // Some syntactically valid private-use tags produce a CultureInfo without usable data.
            _ = formatCulture.NumberFormat.NumberDecimalSeparator;
            return formatCulture;
        }
        catch (Exception error) when (error is CultureNotFoundException or NullReferenceException)
        {
            return CultureInfo.InvariantCulture;
        }
    }

    private RuntimeState CreateState(string culture, string formatCulture)
    {
        var selected = catalog.Select(culture);
        return new RuntimeState(
            culture,
            formatCulture,
            GetFormatCulture(formatCulture),
            selected.RawValues,
            selected.TokenValues
        );
    }

    private sealed record RuntimeState(
        string Culture,
        string FormatCultureName,
        CultureInfo FormatCulture,
        FrozenDictionary<string, Translation> RawValues,
        FrozenDictionary<string, Translation> TokenValues
    );
}
