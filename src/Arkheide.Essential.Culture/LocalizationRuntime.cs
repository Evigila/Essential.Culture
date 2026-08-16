using System.Collections.Frozen;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Arkheide.Essential.Culture;

/// <summary>Owns the process-wide Culture.json catalog and its dynamic culture state.</summary>
internal sealed partial class LocalizationRuntime : ILocalizer
{
    private static readonly Lazy<LocalizationRuntime> SharedRuntime = new(
        static () =>
            new LocalizationRuntime(
                Path.Combine(AppContext.BaseDirectory, "Culture.json"),
                "en-US",
                "en-US"
            ),
        LazyThreadSafetyMode.ExecutionAndPublication
    );
    private readonly Lock gate = new();
    private readonly string sourcePath;
    private readonly string fallback;
    private IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> values =
        new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.Ordinal);
    private IReadOnlyList<string> availableCultures = [];
    private IReadOnlySet<string> keys = new HashSet<string>(StringComparer.Ordinal);
    private string current;

    internal static LocalizationRuntime Shared => SharedRuntime.Value;

    internal LocalizationRuntime(string path, string current, string fallback = "en-US")
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("The file path cannot be empty.", nameof(path));
        }

        sourcePath = Path.GetFullPath(path);
        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException($"File '{sourcePath}' does not exist.", sourcePath);
        }

        this.current = KeyValidation.NormalizeCulture(current, nameof(current));
        this.fallback = KeyValidation.NormalizeCulture(fallback, nameof(fallback));
        var document = LoadDocument(sourcePath, this.fallback);
        ApplyDocument(document);
    }

    internal string SourcePath => sourcePath;

    /// <inheritdoc />
    public string Culture
    {
        get
        {
            lock (gate)
            {
                return current;
            }
        }
    }

    internal string Fallback => fallback;

    /// <inheritdoc />
    public IReadOnlyList<string> AvailableCultures
    {
        get
        {
            lock (gate)
            {
                return availableCultures;
            }
        }
    }

    /// <inheritdoc />
    internal IReadOnlySet<string> Keys
    {
        get
        {
            lock (gate)
            {
                return keys;
            }
        }
    }

    /// <inheritdoc />
    public event EventHandler? Changed;

    public void SetCulture(string culture)
    {
        var normalized = KeyValidation.NormalizeCulture(culture, nameof(culture));
        lock (gate)
        {
            if (string.Equals(current, normalized, StringComparison.Ordinal))
            {
                return;
            }

            current = normalized;
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    public bool Contains(string key)
    {
        if (!KeyToken.TryGetKey(key, out var rawKey))
        {
            return false;
        }

        lock (gate)
        {
            return values.ContainsKey(rawKey);
        }
    }

    public string Parse(string key) => Parse(key, []);

    public string Parse(string key, params object?[] arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        if (!KeyToken.TryGetKey(key, out var rawKey))
        {
            throw new ArgumentException("Value invalid.", nameof(key));
        }

        string resolved;
        string selectedCulture;
        lock (gate)
        {
            if (!values.TryGetValue(rawKey, out var translations))
            {
                resolved = key;
            }
            else
            {
                resolved =
                    TryGetTranslation(translations, current)
                    ?? TryGetTranslation(translations, fallback)
                    ?? key;
            }

            selectedCulture = current;
        }

        return arguments.Length == 0
            ? resolved
            : string.Format(GetFormatCulture(selectedCulture), resolved, arguments);
    }

    internal void Reload()
    {
        var document = LoadDocument(sourcePath, fallback);
        lock (gate)
        {
            ApplyDocument(document);
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void ApplyDocument(ParsedDocument document)
    {
        values = document.Values;
        availableCultures = document.Cultures;
        keys = document.Keys;
    }

    private static ParsedDocument LoadDocument(string path, string fallback)
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

            var entries = new Dictionary<string, IReadOnlyDictionary<string, string>>(
                StringComparer.Ordinal
            );
            HashSet<string>? expectedCultures = null;
            foreach (var entry in document.RootElement.EnumerateObject())
            {
                if (!KeyValidation.IsValidKey(entry.Name))
                {
                    throw Invalid(path, $"contains illegal key '{entry.Name}'.");
                }

                if (!entries.TryAdd(entry.Name, ParseTranslations(path, entry.Name, entry.Value)))
                {
                    throw Invalid(path, $"contains duplicate key '{entry.Name}'");
                }

                var cultures = entries[entry.Name].Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
                if (!cultures.Contains(fallback))
                {
                    throw Invalid(
                        path,
                        $"key '{entry.Name}' does not define fallback culture '{fallback}'"
                    );
                }

                if (expectedCultures is null)
                {
                    expectedCultures = cultures;
                }
                else if (!expectedCultures.SetEquals(cultures))
                {
                    throw Invalid(
                        path,
                        $"key '{entry.Name}' does not define the same culture set as the other keys"
                    );
                }

                ValidateFormatPlaceholders(path, entry.Name, entries[entry.Name], fallback);
            }

            if (entries.Count == 0 || expectedCultures is null)
            {
                throw Invalid(path, "does not contain any translation keys");
            }

            return new ParsedDocument(
                entries,
                Array.AsReadOnly(
                    expectedCultures
                        .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                        .ToArray()
                ),
                entries.Keys.ToFrozenSet(StringComparer.Ordinal)
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

    private static Dictionary<string, string> ParseTranslations(
        string path,
        string key,
        JsonElement element
    )
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw Invalid(path, $"key '{key}' must contain a culture-to-string object");
        }

        var translations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var translation in element.EnumerateObject())
        {
            string culture;
            try
            {
                culture = KeyValidation.NormalizeCulture(translation.Name, translation.Name);
            }
            catch (ArgumentException error)
            {
                throw Invalid(
                    path,
                    $"key '{key}' contains invalid culture '{translation.Name}'",
                    error
                );
            }

            if (
                translation.Value.ValueKind != JsonValueKind.String
                || string.IsNullOrWhiteSpace(translation.Value.GetString())
            )
            {
                throw Invalid(
                    path,
                    $"key '{key}' contains an empty or non-string value for culture '{translation.Name}'"
                );
            }

            if (!translations.TryAdd(culture, translation.Value.GetString()!))
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
        IReadOnlyDictionary<string, string> translations,
        string fallback
    )
    {
        var expected = GetPlaceholderIndexes(translations[fallback]);
        foreach (var (culture, value) in translations)
        {
            if (!expected.SequenceEqual(GetPlaceholderIndexes(value), StringComparer.Ordinal))
            {
                throw Invalid(
                    path,
                    $"key '{key}' does not preserve fallback format placeholders for culture '{culture}'"
                );
            }
        }
    }

    private static string[] GetPlaceholderIndexes(string value) =>
        [
            .. CompositeFormatItemPattern()
                .Matches(value)
                .Select(match => match.Groups["index"].Value)
                .OrderBy(index => index, StringComparer.Ordinal),
        ];

    private static string? TryGetTranslation(
        IReadOnlyDictionary<string, string> translations,
        string culture
    )
    {
        if (translations.TryGetValue(culture, out var exact))
        {
            return exact;
        }

        var separator = culture.LastIndexOf('-');
        while (separator > 0)
        {
            culture = culture[..separator];
            if (translations.TryGetValue(culture, out var parent))
            {
                return parent;
            }

            separator = culture.LastIndexOf('-');
        }

        return null;
    }

    private static CultureInfo GetFormatCulture(string culture)
    {
        try
        {
            return CultureInfo.GetCultureInfo(culture);
        }
        catch (CultureNotFoundException)
        {
            return CultureInfo.InvariantCulture;
        }
    }

    private static InvalidDataException Invalid(
        string path,
        string message,
        Exception? inner = null
    ) => new($"'{path}' {message}.", inner);

    [GeneratedRegex(
        @"(?<!\{)\{(?<index>\d+)(?:,[^{}]+)?(?::[^{}]+)?\}(?!\})",
        RegexOptions.CultureInvariant
    )]
    private static partial Regex CompositeFormatItemPattern();

    private sealed record ParsedDocument(
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> Values,
        IReadOnlyList<string> Cultures,
        IReadOnlySet<string> Keys
    );
}
