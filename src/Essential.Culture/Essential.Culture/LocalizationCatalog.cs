using System.Collections.Frozen;
using System.Text;
using System.Text.Json;

namespace ArkheideSystem.Essential.Culture;

/// <summary>
/// Owns validated immutable translations. Catalogs can be shared while each localization context
/// retains its own selected culture.
/// </summary>
public sealed class LocalizationCatalog
{
    private readonly Catalog catalog;
    private readonly Lock cacheGate = new();
    private readonly Dictionary<string, CatalogSelection> selections =
        new(StringComparer.OrdinalIgnoreCase);

    private LocalizationCatalog(Catalog catalog) => this.catalog = catalog;

    /// <summary>Gets the normalized cultures declared by every key in this catalog.</summary>
    public IReadOnlyList<string> AvailableCultures => catalog.Cultures;

    /// <summary>Gets the configured fallback translation culture.</summary>
    public string FallbackCulture => catalog.Fallback;

    /// <summary>Parses and validates a JSON catalog without reading a file.</summary>
    public static LocalizationCatalog FromJson(string json, string fallbackCulture = "en-US")
    {
        ArgumentNullException.ThrowIfNull(json);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        return Load(stream, fallbackCulture);
    }

    /// <summary>
    /// Reads and validates a catalog from the stream's current position. The caller owns the stream;
    /// this method does not close it, including when validation fails.
    /// </summary>
    public static LocalizationCatalog Load(Stream stream, string fallbackCulture = "en-US")
    {
        ArgumentNullException.ThrowIfNull(stream);
        var fallback = KeyValidation.NormalizeCulture(fallbackCulture, nameof(fallbackCulture));
        return new LocalizationCatalog(ReadDocument(stream, "<stream>", fallback));
    }

    /// <summary>Reads and validates a catalog from an explicitly supplied file path.</summary>
    public static LocalizationCatalog FromFile(string path, string fallbackCulture = "en-US")
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

        var fallback = KeyValidation.NormalizeCulture(fallbackCulture, nameof(fallbackCulture));
        return LoadValidatedFile(sourcePath, fallback);
    }

    internal static LocalizationCatalog LoadValidatedFile(string path, string fallback)
    {
        try
        {
            using var stream = File.OpenRead(path);
            return new LocalizationCatalog(ReadDocument(stream, path, fallback));
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            throw Invalid(path, "could not be read", error);
        }
    }

    internal CatalogSelection Select(string culture)
    {
        lock (cacheGate)
        {
            if (selections.TryGetValue(culture, out var selected))
            {
                return selected;
            }

            var cultureChain = GetCultureChain(culture, catalog.Fallback);
            var rawValues = new Dictionary<string, Translation>(catalog.Entries.Count, StringComparer.Ordinal);
            var tokenValues = new Dictionary<string, Translation>(catalog.Entries.Count, StringComparer.Ordinal);
            foreach (var (key, entry) in catalog.Entries)
            {
                var translation = SelectTranslation(entry.Translations, cultureChain);
                rawValues.Add(key, translation);
                tokenValues.Add(entry.Token, translation);
            }

            selected = new CatalogSelection(
                rawValues.ToFrozenDictionary(StringComparer.Ordinal),
                tokenValues.ToFrozenDictionary(StringComparer.Ordinal)
            );
            // Only declared cultures form the shared cache. Arbitrary/custom requests stay bounded.
            if (catalog.DeclaredCultures.Contains(culture))
            {
                selections.Add(culture, selected);
            }

            return selected;
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

    private static Catalog ReadDocument(Stream stream, string path, string fallback)
    {
        try
        {
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

    internal sealed record Translation(string Text, CompositeFormat Format);

    internal sealed record CatalogSelection(
        FrozenDictionary<string, Translation> RawValues,
        FrozenDictionary<string, Translation> TokenValues
    );
}
