using System.Collections.Concurrent;
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
    private readonly ConcurrentDictionary<string, Lazy<CatalogSelection>> selections =
        new(StringComparer.OrdinalIgnoreCase);

    private LocalizationCatalog(Catalog catalog) => this.catalog = catalog;

    /// <summary>Gets selectable culture names, or all declared names when no policy is configured.</summary>
    public IReadOnlyList<string> AvailableCultures => catalog.AvailableCultures;

    /// <summary>Gets the union of normalized culture names authored in all catalog documents.</summary>
    public IReadOnlyList<string> DeclaredCultures => catalog.DeclaredCultures;

    /// <summary>Gets the configured fallback translation culture.</summary>
    public string FallbackCulture => catalog.Fallback;

    /// <summary>Parses and validates a JSON catalog without reading a file.</summary>
    public static LocalizationCatalog FromJson(string json, string fallbackCulture = "en-US")
    {
        ArgumentNullException.ThrowIfNull(json);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        return Load(stream, fallbackCulture);
    }

    /// <summary>Parses and fully validates JSON while retaining only the configured languages.</summary>
    public static LocalizationCatalog FromJson(
        string json, string fallbackCulture, CatalogLoadOptions options
    )
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentNullException.ThrowIfNull(options);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        return Load(stream, fallbackCulture, options);
    }

    /// <summary>
    /// Reads and validates a catalog from the stream's current position. The caller owns the stream;
    /// this method does not close it, including when validation fails.
    /// </summary>
    public static LocalizationCatalog Load(Stream stream, string fallbackCulture = "en-US")
    {
        ArgumentNullException.ThrowIfNull(stream);
        var fallback = KeyValidation.NormalizeCulture(fallbackCulture, nameof(fallbackCulture));
        return LoadDocument(stream, "<stream>", fallback, null);
    }

    /// <summary>Fully validates the stream and filters retained translations without closing it.</summary>
    public static LocalizationCatalog Load(
        Stream stream, string fallbackCulture, CatalogLoadOptions options
    )
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(options);
        var fallback = KeyValidation.NormalizeCulture(fallbackCulture, nameof(fallbackCulture));
        return LoadDocument(stream, "<stream>", fallback, options);
    }

    /// <summary>Reads and validates a catalog from an explicitly supplied file path.</summary>
    public static LocalizationCatalog FromFile(string path, string fallbackCulture = "en-US")
    {
        var sourcePath = ValidatePath(path);
        var fallback = KeyValidation.NormalizeCulture(fallbackCulture, nameof(fallbackCulture));
        return LoadValidatedFile(sourcePath, fallback);
    }

    /// <summary>Fully validates a file and applies an immutable retained-language policy.</summary>
    public static LocalizationCatalog FromFile(
        string path, string fallbackCulture, CatalogLoadOptions options
    )
    {
        ArgumentNullException.ThrowIfNull(options);
        var sourcePath = ValidatePath(path);
        var fallback = KeyValidation.NormalizeCulture(fallbackCulture, nameof(fallbackCulture));
        var policy = CreatePolicy(fallback, options);
        return CreateCatalog(ReadFile(sourcePath, fallback, policy), fallback, options);
    }

    /// <summary>
    /// Eagerly composes explicitly supplied modules with globally unique keys. Each document must
    /// have consistent per-key languages and the fallback; optional languages may differ by module.
    /// </summary>
    public static LocalizationCatalog FromFiles(
        IEnumerable<string> paths,
        string fallbackCulture = "en-US",
        CatalogLoadOptions? options = null
    )
    {
        ArgumentNullException.ThrowIfNull(paths);
        var fallback = KeyValidation.NormalizeCulture(fallbackCulture, nameof(fallbackCulture));
        var policy = CreatePolicy(fallback, options);
        var seenPaths = new HashSet<string>(
            OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal
        );
        var entries = new Dictionary<string, CatalogEntry>(StringComparer.Ordinal);
        var sources = new Dictionary<string, string>(StringComparer.Ordinal);
        var declared = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var retained = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in paths)
        {
            var sourcePath = ValidatePath(path);
            if (!seenPaths.Add(sourcePath))
            {
                throw new ArgumentException($"Module path '{sourcePath}' is supplied more than once.", nameof(paths));
            }

            var module = ReadFile(sourcePath, fallback, policy);
            foreach (var (key, entry) in module.Entries)
            {
                if (!entries.TryAdd(key, entry))
                {
                    throw Invalid(sourcePath, $"contains duplicate key '{key}' also declared in '{sources[key]}'");
                }
                sources.Add(key, sourcePath);
            }
            declared.UnionWith(module.DeclaredCultures);
            retained.UnionWith(module.RetainedCultures);
        }

        if (seenPaths.Count == 0)
        {
            throw new ArgumentException("At least one module path is required.", nameof(paths));
        }

        return CreateCatalog(new Document(entries, declared, retained), fallback, options);
    }

    /// <summary>
    /// Tests a normalized exact selection against the configured policy. Without a policy, any
    /// valid tag is enabled for legacy parent/fallback resolution. Invalid names throw.
    /// </summary>
    public bool IsCultureEnabled(string culture) =>
        IsNormalizedCultureEnabled(KeyValidation.NormalizeCulture(culture, nameof(culture)));

    internal bool IsNormalizedCultureEnabled(string culture) =>
        !catalog.HasPolicy || catalog.SelectableCultures.Contains(culture);

    internal static LocalizationCatalog LoadValidatedFile(string path, string fallback)
    {
        return CreateCatalog(ReadFile(path, fallback, CreatePolicy(fallback, null)), fallback, null);
    }

    internal CatalogSelection Select(string culture)
    {
        if (!IsNormalizedCultureEnabled(culture))
        {
            throw new ArgumentException($"Culture '{culture}' is not enabled by this catalog.", nameof(culture));
        }

        // All requests with the same deepest retained parent have the same per-module chain.
        // Cache only retained names, never arbitrary requested tags.
        var effective = GetEffectiveCulture(culture);
        if (selections.TryGetValue(effective, out var selected)) return selected.Value;
        return selections.GetOrAdd(effective, name => new Lazy<CatalogSelection>(
            () => BuildSelection(name), LazyThreadSafetyMode.ExecutionAndPublication
        )).Value;
    }

    private string GetEffectiveCulture(string culture)
    {
        while (true)
        {
            if (catalog.RetainedCultures.Contains(culture)) return culture;
            var separator = culture.LastIndexOf('-');
            if (separator <= 0) return catalog.Fallback;
            culture = culture[..separator];
        }
    }

    private CatalogSelection BuildSelection(string culture)
    {
        var cultureChain = GetCultureChain(culture, catalog.Fallback);
        var rawValues = new Dictionary<string, Translation>(catalog.Entries.Count, StringComparer.Ordinal);
        var tokenValues = new Dictionary<string, Translation>(catalog.Entries.Count, StringComparer.Ordinal);
        foreach (var (key, entry) in catalog.Entries)
        {
            var translation = SelectTranslation(entry.Translations, cultureChain);
            rawValues.Add(key, translation);
            tokenValues.Add(entry.Token, translation);
        }

        return new CatalogSelection(
            rawValues.ToFrozenDictionary(StringComparer.Ordinal),
            tokenValues.ToFrozenDictionary(StringComparer.Ordinal)
        );
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

    private static string ValidatePath(string path)
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
        return sourcePath;
    }

    private static LocalizationCatalog LoadDocument(
        Stream stream, string path, string fallback, CatalogLoadOptions? options
    ) => CreateCatalog(ReadDocument(stream, path, fallback, CreatePolicy(fallback, options)), fallback, options);

    private static Document ReadFile(string path, string fallback, LoadPolicy policy)
    {
        try
        {
            using var stream = File.OpenRead(path);
            return ReadDocument(stream, path, fallback, policy);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            throw Invalid(path, "could not be read", error);
        }
    }

    private static LoadPolicy CreatePolicy(string fallback, CatalogLoadOptions? options)
    {
        if (options is null) return new LoadPolicy(null, null);
        if (options.DisabledNames.Contains(fallback))
        {
            throw new ArgumentException($"Fallback culture '{fallback}' cannot be disabled.", nameof(options));
        }
        HashSet<string>? retained = null;
        if (options.EnabledCultures is not null)
        {
            retained = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { fallback };
            foreach (var enabled in options.EnabledCultures)
            {
                var name = enabled;
                while (true)
                {
                    if (!options.DisabledNames.Contains(name)) retained.Add(name);
                    var separator = name.LastIndexOf('-');
                    if (separator <= 0) break;
                    name = name[..separator];
                }
            }
        }
        return new LoadPolicy(retained, options.DisabledNames);
    }

    private static LocalizationCatalog CreateCatalog(Document document, string fallback, CatalogLoadOptions? options)
    {
        var declared = document.DeclaredCultures.OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray();
        var available = options?.EnabledCultures?.ToArray()
            ?? declared.Where(name => options is null || !options.DisabledNames.Contains(name)).ToArray();
        return new LocalizationCatalog(new Catalog(
            document.Entries.ToFrozenDictionary(StringComparer.Ordinal),
            Array.AsReadOnly(declared),
            Array.AsReadOnly(available),
            available.ToFrozenSet(StringComparer.OrdinalIgnoreCase),
            document.RetainedCultures.ToFrozenSet(StringComparer.OrdinalIgnoreCase),
            options?.HasPolicy ?? false,
            fallback
        ));
    }

    private static Document ReadDocument(Stream stream, string path, string fallback, LoadPolicy policy)
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
            var cultureNames = new Dictionary<string, string>(StringComparer.Ordinal);
            HashSet<string>? expectedCultures = null;
            foreach (var property in document.RootElement.EnumerateObject())
            {
                var key = property.Name;
                if (!KeyValidation.IsValidKey(key))
                {
                    throw Invalid(path, $"contains illegal key '{key}'");
                }

                var translations = ParseTranslations(path, key, property.Value, cultureNames);
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
                var retainedTranslations = policy.HasFilter
                    ? translations.Where(pair => policy.ShouldRetain(pair.Key))
                    : translations;
                if (
                    !entries.TryAdd(
                        key,
                        new CatalogEntry(
                            KeyToken.Prefix + key,
                            retainedTranslations.ToFrozenDictionary(
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

            var retainedCultures = policy.HasFilter
                ? expectedCultures.Where(policy.ShouldRetain).ToHashSet(StringComparer.OrdinalIgnoreCase)
                : expectedCultures;
            return new Document(entries, expectedCultures, retainedCultures);
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
        JsonElement element,
        Dictionary<string, string> cultureNames
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
            var authoredCulture = property.Name;
            string culture;
            try
            {
                if (!cultureNames.TryGetValue(authoredCulture, out culture!))
                {
                    culture = KeyValidation.NormalizeCulture(authoredCulture, authoredCulture);
                    cultureNames.Add(authoredCulture, culture);
                }
            }
            catch (ArgumentException error)
            {
                throw Invalid(
                    path,
                    $"key '{key}' contains invalid culture '{authoredCulture}'",
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
                    $"key '{key}' contains an empty or non-string value for culture '{authoredCulture}'"
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
                    $"key '{key}' contains an invalid composite format for culture '{authoredCulture}'",
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
        IReadOnlyList<string> DeclaredCultures,
        IReadOnlyList<string> AvailableCultures,
        FrozenSet<string> SelectableCultures,
        FrozenSet<string> RetainedCultures,
        bool HasPolicy,
        string Fallback
    );

    private sealed record Document(
        Dictionary<string, CatalogEntry> Entries,
        HashSet<string> DeclaredCultures,
        HashSet<string> RetainedCultures
    );

    private sealed record LoadPolicy(HashSet<string>? RetainedNames, FrozenSet<string>? DisabledNames)
    {
        internal bool HasFilter => RetainedNames is not null || DisabledNames is { Count: > 0 };

        internal bool ShouldRetain(string culture) =>
            (DisabledNames is null || !DisabledNames.Contains(culture))
            && (RetainedNames is null || RetainedNames.Contains(culture));
    }

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
