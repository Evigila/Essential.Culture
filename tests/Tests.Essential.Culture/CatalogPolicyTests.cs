using System.Reflection;
using System.Text;

namespace ArkheideSystem.Essential.Culture.Test;

public sealed class CatalogPolicyTests
{
    private const string Json = """
        {
          "Greeting": { "en-US": "Hello", "fr": "Bonjour", "ja-JP": "Discarded Japanese" },
          "Amount": { "en-US": "Amount {0:N2}", "fr": "Montant {0:N2}", "ja-JP": "Discarded {0:N2}" }
        }
        """;

    [Fact]
    public void OptionsCopyNormalizeAndRejectContradictoryLists()
    {
        var enabled = new[] { " fr_CA ", "en_US", "fr-CA" };
        var disabled = new[] { "ja_JP" };
        var options = new CatalogLoadOptions(enabled, disabled);
        enabled[0] = "ja-JP";
        disabled[0] = "en-US";
        Assert.Equal(["en-US", "fr-CA"], options.EnabledCultures);
        Assert.Equal(["ja-JP"], options.DisabledCultures);
        Assert.Throws<ArgumentException>(() => new CatalogLoadOptions(["fr_CA"], ["fr-CA"]));
        Assert.Throws<ArgumentException>(() => new CatalogLoadOptions(disabledCultures: ["invalid!"]));
        Assert.Throws<NotSupportedException>(() => ((IList<string>)options.DisabledCultures)[0] = "en-US");
    }

    [Fact]
    public void AllowedChildrenKeepNeededParentsWithoutMakingTheParentsSelectable()
    {
        var catalog = LocalizationCatalog.FromJson(Json, "en-US", new CatalogLoadOptions(["fr-CA", "x-demo"]));
        Assert.Equal(["en-US", "fr", "ja-JP"], catalog.DeclaredCultures);
        Assert.Equal(["fr-CA", "x-Demo"], catalog.AvailableCultures);
        Assert.Equal(["en-US", "fr"], RetainedCultures(catalog));
        var context = new LocalizationContext(catalog, "fr_CA", "de-DE");
        Assert.Equal("Bonjour", context.Parse("Key.Greeting"));
        Assert.Equal("Montant 1.234,50", context.Parse("Amount", 1234.5m));
        Assert.Same(catalog.DeclaredCultures, context.DeclaredCultures);
        Assert.True(catalog.IsCultureEnabled(" fr_CA "));
        Assert.False(catalog.IsCultureEnabled("fr"));
        var events = 0;
        context.Changed += (_, _) => events++;
        Assert.Throws<ArgumentException>(() => context.SetCulture("fr"));
        Assert.Equal("fr-CA", context.Culture);
        Assert.Equal(0, events);
        context.SetCulture("x-demo");
        Assert.Equal("Hello", context.Parse("Greeting"));
        Assert.Equal(1, events);
    }

    [Fact]
    public void AnExactlyDeniedParentIsNeitherLoadedNorUsedByAnAllowedChild()
    {
        var catalog = LocalizationCatalog.FromJson(Json, "en-US", new CatalogLoadOptions(["fr-CA"], ["fr"]));
        Assert.Equal(["en-US"], RetainedCultures(catalog));
        Assert.Equal("Hello", new LocalizationContext(catalog, "fr-CA").Parse("Greeting"));
        Assert.False(catalog.IsCultureEnabled("fr"));
    }

    [Fact]
    public void DenyPolicyRejectsDirectAndUndeclaredSelectionsButPreservesFormattingIndependence()
    {
        var catalog = LocalizationCatalog.FromJson(Json, "en-US", new CatalogLoadOptions(disabledCultures: ["ja_JP"]));
        Assert.Equal(["en-US", "fr"], catalog.AvailableCultures);
        Assert.Equal(["en-US", "fr"], RetainedCultures(catalog));
        Assert.False(catalog.IsCultureEnabled("ja-JP"));
        Assert.False(catalog.IsCultureEnabled("fr-CA"));
        Assert.Throws<ArgumentException>(() => new LocalizationContext(catalog, "ja-JP"));
        Assert.Equal("Hello", new LocalizationContext(catalog, "en-US", "ja-JP").Parse("Greeting"));
        Assert.Throws<ArgumentException>(() => catalog.IsCultureEnabled("invalid!"));
    }

    [Fact]
    public void FallbackCannotBeDisabledButCanBeReplacedExplicitly()
    {
        var options = new CatalogLoadOptions(disabledCultures: ["en_US"]);
        Assert.Throws<ArgumentException>(() => LocalizationCatalog.FromJson(Json, "en-US", options));
        var catalog = LocalizationCatalog.FromJson(Json, "fr", options);
        Assert.Equal("fr", catalog.FallbackCulture);
        Assert.Equal("Bonjour", new LocalizationContext(catalog, "fr").Parse("Greeting"));
        Assert.Equal(["fr", "ja-JP"], RetainedCultures(catalog));
    }

    [Theory]
    [InlineData("{broken}")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("{1}")]
    public void ExcludedTranslationsStillUndergoFullTextAndFormatValidation(string discarded)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, Dictionary<string, string>>
        {
            ["Greeting"] = new() { ["en-US"] = "Hello {0}", ["ja-JP"] = discarded },
        });
        Assert.Throws<InvalidDataException>(() => LocalizationCatalog.FromJson(
            json, "en-US", new CatalogLoadOptions(["en-US"])
        ));
    }

    [Theory]
    [InlineData("{\"Greeting\":{\"en-US\":\"Hello\",\"ja-JP\":false}}")]
    [InlineData("{\"Greeting\":{\"en-US\":\"Hello\",\"ja-JP\":\"A\",\"ja_JP\":\"B\"}}")]
    [InlineData("{\"A\":{\"en-US\":\"A\",\"ja-JP\":\"A\"},\"B\":{\"en-US\":\"B\"}}")]
    public void ExcludedTranslationsStillValidateTypesDuplicatesAndPerKeyCultureSets(string json)
    {
        Assert.Throws<InvalidDataException>(() => LocalizationCatalog.FromJson(
            json, "en-US", new CatalogLoadOptions(disabledCultures: ["ja-JP"])
        ));
    }

    [Fact]
    public void UnconfiguredAndEmptyOptionsKeepLegacyUndeclaredFallbackBehavior()
    {
        foreach (var catalog in new[]
        {
            LocalizationCatalog.FromJson(Json),
            LocalizationCatalog.FromJson(Json, "en-US", new CatalogLoadOptions()),
        })
        {
            Assert.True(catalog.IsCultureEnabled("fr-CA"));
            Assert.True(catalog.IsCultureEnabled("x-demo"));
            Assert.Equal("Bonjour", new LocalizationContext(catalog, "fr-CA").Parse("Greeting"));
            Assert.Equal("Hello", new LocalizationContext(catalog, "x-demo").Parse("Greeting"));
            Assert.Equal(catalog.DeclaredCultures, catalog.AvailableCultures);
        }
    }

    [Fact]
    public void RepeatedAndDifferentAuthoredCultureSpellingsNormalizeConsistentlyAcrossKeys()
    {
        var catalog = LocalizationCatalog.FromJson("""
            {"First":{"en_US":"First","fr":"Premier"},
             "Second":{"en-US":"Second","FR":"Deuxième"},
             "Third":{"en_US":"Third","fr":"Troisième"}}
            """, "en-US", new CatalogLoadOptions(["fr-CA"]));
        Assert.Equal(["en-US", "fr"], catalog.DeclaredCultures);
        var context = new LocalizationContext(catalog, "fr-CA");
        Assert.Equal("Premier", context.Parse("First"));
        Assert.Equal("Deuxième", context.Parse("Second"));
        Assert.Equal("Troisième", context.Parse("Third"));
        Assert.Equal(["en-US", "fr"], RetainedCultures(catalog));
    }

    [Fact]
    public void ExplicitEmptyAllowlistRetainsFallbackButEnablesNoSelection()
    {
        var catalog = LocalizationCatalog.FromJson(Json, "en-US", new CatalogLoadOptions([]));
        Assert.Empty(catalog.AvailableCultures);
        Assert.Equal(["en-US"], RetainedCultures(catalog));
        Assert.False(catalog.IsCultureEnabled("en-US"));
        Assert.Throws<ArgumentException>(() => new LocalizationContext(catalog));
    }

    [Fact]
    public void FilteredStreamLoadingPreservesCurrentPositionAndCallerOwnershipOnSuccessAndFailure()
    {
        var options = new CatalogLoadOptions(["en-US"]);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("ignored" + Json));
        stream.Position = "ignored".Length;
        var catalog = LocalizationCatalog.Load(stream, "en-US", options);
        Assert.True(stream.CanRead);
        Assert.Equal(["en-US"], RetainedCultures(catalog));
        using var invalid = new MemoryStream(Encoding.UTF8.GetBytes("{}"));
        Assert.Throws<InvalidDataException>(() => LocalizationCatalog.Load(invalid, "en-US", options));
        Assert.True(invalid.CanRead);
    }

    [Fact]
    public async Task EffectiveSelectionsAreSharedUnderConcurrentParentAndArbitraryFallbackRequests()
    {
        var catalog = LocalizationCatalog.FromJson(Json);
        await Task.WhenAll(Enumerable.Range(0, 100).Select(index => Task.Run(() =>
        {
            var context = new LocalizationContext(catalog, index % 2 == 0 ? $"fr-X{index}" : $"x-Custom{index}");
            Assert.Equal(index % 2 == 0 ? "Bonjour" : "Hello", context.Parse("Greeting"));
        })));
        var cache = typeof(LocalizationCatalog).GetField("selections", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(catalog)!;
        Assert.Equal(2, (int)cache.GetType().GetProperty("Count")!.GetValue(cache)!);
    }

    internal static string[] RetainedCultures(LocalizationCatalog catalog)
    {
        // Inspect retained translation state, rather than the public authored-name metadata.
        var data = typeof(LocalizationCatalog).GetField("catalog", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(catalog)!;
        var entries = (System.Collections.IEnumerable)data.GetType().GetProperty("Entries")!.GetValue(data)!;
        var retained = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in entries)
        {
            var entry = pair.GetType().GetProperty("Value")!.GetValue(pair)!;
            var translations = (System.Collections.IEnumerable)entry.GetType().GetProperty("Translations")!.GetValue(entry)!;
            foreach (var translation in translations)
                retained.Add((string)translation.GetType().GetProperty("Key")!.GetValue(translation)!);
        }
        return retained.OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray();
    }
}
