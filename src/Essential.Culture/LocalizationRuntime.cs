using System.Collections.Frozen;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace ArkheideSystem.Essential.Culture;

/// <summary>Owns the process-wide Culture.json catalog and its dynamic culture state.</summary>
internal sealed class LocalizationRuntime
{
    private static class SharedHolder
    {
        internal static readonly LocalizationRuntime Instance = new(
                Path.Combine(AppContext.BaseDirectory, "Culture.json"),
                "en-US",
                "en-US"
            );
    }

    private readonly Lock stateGate = new();
    private readonly Catalog catalog;
    private readonly Dictionary<string, RuntimeState> stateCache =
        new(StringComparer.OrdinalIgnoreCase);
    private RuntimeState state;

    internal static LocalizationRuntime Shared => SharedHolder.Instance;

    internal LocalizationRuntime(string path, string current, string fallback = "en-US")
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("The file path cannot be empty.", nameof(path));
        }

        var sourcePath = Path.GetFullPath(path);
        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException($"File '{sourcePath}' does not exist.", sourcePath);
        }

        var normalizedFallback = KeyValidation.NormalizeCulture(fallback, nameof(fallback));
        catalog = LoadDocument(sourcePath, normalizedFallback);
        state = CreateState(
            catalog,
            KeyValidation.NormalizeCulture(current, nameof(current))
        );
        CacheStateIfBounded(state);
    }

    internal string Culture => Volatile.Read(ref state).Culture;

    internal IReadOnlyList<string> AvailableCultures => catalog.Cultures;

    internal event EventHandler? Changed;

    internal void SetCulture(string culture)
    {
        var normalized = KeyValidation.NormalizeCulture(culture, nameof(culture));
        lock (stateGate)
        {
            if (string.Equals(state.Culture, normalized, StringComparison.Ordinal))
            {
                return;
            }

            if (!stateCache.TryGetValue(normalized, out var next))
            {
                next = CreateState(catalog, normalized);
                CacheStateIfBounded(next);
            }

            Volatile.Write(ref state, next);
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    internal bool Contains(string token) => TryResolve(Volatile.Read(ref state), token, out _);

    internal bool TryParse(string token, out string value)
    {
        if (TryResolve(Volatile.Read(ref state), token, out var resolved))
        {
            value = resolved.Text;
            return true;
        }

        value = string.Empty;
        return false;
    }

    internal bool TryParse(string token, object?[] arguments, out string value)
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

    internal bool TryParse<TArg0>(string token, TArg0 argument0, out string value)
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

    internal bool TryParse<TArg0, TArg1>(
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

    internal bool TryParse<TArg0, TArg1, TArg2>(
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

    internal string Parse(string token)
    {
        var resolved = ResolveOrFallback(Volatile.Read(ref state), token);
        return resolved?.Text ?? token;
    }

    internal string Parse(string token, params object?[] arguments)
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

    internal string Parse<TArg0>(string token, TArg0 argument0)
    {
        var snapshot = Volatile.Read(ref state);
        var resolved = ResolveOrFallback(snapshot, token);
        return resolved is null
            ? token
            : string.Format(snapshot.FormatCulture, resolved.Format, argument0);
    }

    internal string Parse<TArg0, TArg1>(string token, TArg0 argument0, TArg1 argument1)
    {
        var snapshot = Volatile.Read(ref state);
        var resolved = ResolveOrFallback(snapshot, token);
        return resolved is null
            ? token
            : string.Format(snapshot.FormatCulture, resolved.Format, argument0, argument1);
    }

    internal string Parse<TArg0, TArg1, TArg2>(
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

    private static RuntimeState CreateState(Catalog catalog, string culture)
    {
        var cultureChain = GetCultureChain(culture, catalog.Fallback);
        var rawValues = new Dictionary<string, Translation>(catalog.Entries.Count, StringComparer.Ordinal);
        var tokenValues = new Dictionary<string, Translation>(catalog.Entries.Count, StringComparer.Ordinal);
        foreach (var (key, entry) in catalog.Entries)
        {
            var selected = SelectTranslation(entry.Translations, cultureChain);
            rawValues.Add(key, selected);
            tokenValues.Add(entry.Token, selected);
        }

        return new RuntimeState(
            culture,
            GetFormatCulture(culture),
            rawValues.ToFrozenDictionary(StringComparer.Ordinal),
            tokenValues.ToFrozenDictionary(StringComparer.Ordinal)
        );
    }

    private void CacheStateIfBounded(RuntimeState snapshot)
    {
        // Declared cultures form a naturally bounded cache and cover normal UI toggles.
        if (catalog.DeclaredCultures.Contains(snapshot.Culture))
        {
            stateCache.TryAdd(snapshot.Culture, snapshot);
        }
    }

    private static Translation SelectTranslation(
        FrozenDictionary<string, Translation> translations,
        IReadOnlyList<string> cultureChain
    )
    {
        foreach (var culture in cultureChain)
        {
            if (translations.TryGetValue(culture, out var translation))
            {
                return translation;
            }
        }

        throw new InvalidOperationException("The validated fallback translation is missing.");
    }

    private static IReadOnlyList<string> GetCultureChain(string culture, string fallback)
    {
        var result = new List<string>(6);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        AddCultureAndParents(culture, result, seen);
        AddCultureAndParents(fallback, result, seen);
        return result;
    }

    private static void AddCultureAndParents(
        string culture,
        List<string> result,
        HashSet<string> seen
    )
    {
        while (seen.Add(culture))
        {
            result.Add(culture);
            var separator = culture.LastIndexOf('-');
            if (separator <= 0)
            {
                return;
            }

            culture = culture[..separator];
        }
    }

    private static Catalog LoadDocument(string path, string fallback)
    {
        try
        {
            using var stream = File.OpenRead(path);
            using var document = JsonDocument.Parse(
                stream,
                new JsonDocumentOptions
                {
                    AllowTrailingCommas = false,
                    CommentHandling = JsonCommentHandling.Disallow,
                }
            );
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw Invalid(path, "must contain a JSON object at its root");
            }

            var entries = new Dictionary<string, CatalogEntry>(StringComparer.Ordinal);
            HashSet<string>? expectedCultures = null;
            foreach (var property in document.RootElement.EnumerateObject())
            {
                var key = property.Name;
                if (!KeyValidation.IsValidKey(key))
                {
                    throw Invalid(path, $"contains illegal key '{key}'");
                }

                var translations = ParseTranslations(path, key, property.Value);
                if (!translations.ContainsKey(fallback))
                {
                    throw Invalid(
                        path,
                        $"key '{key}' does not define fallback culture '{fallback}'"
                    );
                }

                if (expectedCultures is null)
                {
                    expectedCultures = new HashSet<string>(
                        translations.Keys,
                        StringComparer.OrdinalIgnoreCase
                    );
                }
                else if (
                    expectedCultures.Count != translations.Count
                    || translations.Keys.Any(culture => !expectedCultures.Contains(culture))
                )
                {
                    throw Invalid(
                        path,
                        $"key '{key}' does not define the same culture set as the other keys"
                    );
                }

                ValidateFormatPlaceholders(path, key, translations, fallback);
                if (
                    !entries.TryAdd(
                        key,
                        new CatalogEntry(
                            KeyToken.Prefix + key,
                            translations.ToFrozenDictionary(
                                pair => pair.Key,
                                pair => new Translation(pair.Value.Text, pair.Value.Format),
                                StringComparer.OrdinalIgnoreCase
                            )
                        )
                    )
                )
                {
                    throw Invalid(path, $"contains duplicate key '{key}'");
                }
            }

            if (entries.Count == 0 || expectedCultures is null)
            {
                throw Invalid(path, "does not contain any translation keys");
            }

            return new Catalog(
                entries.ToFrozenDictionary(StringComparer.Ordinal),
                Array.AsReadOnly(
                    expectedCultures
                        .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                        .ToArray()
                ),
                expectedCultures.ToFrozenSet(StringComparer.OrdinalIgnoreCase),
                fallback
            );
        }
        catch (InvalidDataException)
        {
            throw;
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"'{path}' contains invalid JSON.", error);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            throw new InvalidDataException($"'{path}' could not be read.", error);
        }
    }

    private static Dictionary<string, ParsedTranslation> ParseTranslations(
        string path,
        string key,
        JsonElement element
    )
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw Invalid(path, $"key '{key}' must contain a culture-to-string object");
        }

        var translations = new Dictionary<string, ParsedTranslation>(
            StringComparer.OrdinalIgnoreCase
        );
        foreach (var property in element.EnumerateObject())
        {
            string culture;
            try
            {
                culture = KeyValidation.NormalizeCulture(property.Name, property.Name);
            }
            catch (ArgumentException error)
            {
                throw Invalid(
                    path,
                    $"key '{key}' contains invalid culture '{property.Name}'",
                    error
                );
            }

            var value = property.Value.ValueKind == JsonValueKind.String
                ? property.Value.GetString()
                : null;
            if (string.IsNullOrWhiteSpace(value))
            {
                throw Invalid(
                    path,
                    $"key '{key}' contains an empty or non-string value for culture '{property.Name}'"
                );
            }

            CompositeFormat format;
            try
            {
                format = CompositeFormat.Parse(value);
            }
            catch (FormatException error)
            {
                throw Invalid(
                    path,
                    $"key '{key}' contains an invalid composite format for culture '{property.Name}'",
                    error
                );
            }

            if (
                !translations.TryAdd(
                    culture,
                    new ParsedTranslation(value, format, GetPlaceholderIndexes(value))
                )
            )
            {
                throw Invalid(
                    path,
                    $"key '{key}' contains duplicate normalized culture '{culture}'"
                );
            }
        }

        if (translations.Count == 0)
        {
            throw Invalid(path, $"key '{key}' does not contain any translations");
        }

        return translations;
    }

    private static void ValidateFormatPlaceholders(
        string path,
        string key,
        IReadOnlyDictionary<string, ParsedTranslation> translations,
        string fallback
    )
    {
        var expected = translations[fallback].PlaceholderIndexes;
        foreach (var (culture, translation) in translations)
        {
            if (!expected.AsSpan().SequenceEqual(translation.PlaceholderIndexes))
            {
                throw Invalid(
                    path,
                    $"key '{key}' does not preserve fallback format placeholders for culture '{culture}'"
                );
            }
        }
    }

    private static int[] GetPlaceholderIndexes(string value)
    {
        var indexes = new List<int>();
        for (var position = 0; position < value.Length; position++)
        {
            if (value[position] != '{')
            {
                continue;
            }

            if (position + 1 < value.Length && value[position + 1] == '{')
            {
                position++;
                continue;
            }

            var index = 0;
            position++;
            while (position < value.Length && char.IsDigit(value[position]))
            {
                index = checked(index * 10 + value[position] - '0');
                position++;
            }

            indexes.Add(index);
        }

        indexes.Sort();
        return [.. indexes];
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

    private static InvalidDataException Invalid(
        string path,
        string message,
        Exception? inner = null
    ) => new($"'{path}' {message}.", inner);

    private sealed record Catalog(
        FrozenDictionary<string, CatalogEntry> Entries,
        IReadOnlyList<string> Cultures,
        FrozenSet<string> DeclaredCultures,
        string Fallback
    );

    private sealed record CatalogEntry(
        string Token,
        FrozenDictionary<string, Translation> Translations
    );

    private sealed record ParsedTranslation(
        string Text,
        CompositeFormat Format,
        int[] PlaceholderIndexes
    );

    private sealed record Translation(string Text, CompositeFormat Format);

    private sealed record RuntimeState(
        string Culture,
        CultureInfo FormatCulture,
        FrozenDictionary<string, Translation> RawValues,
        FrozenDictionary<string, Translation> TokenValues
    );
}
