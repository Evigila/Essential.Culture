using System.Globalization;
using System.Text;

namespace ArkheideSystem.Essential.Culture.Test;

public sealed class LocalizationContextTests
{
    private const string CatalogJson = """
        {
          "Greeting": { "en-US": "Hello", "fr": "Bonjour", "de-DE": "Hallo" },
          "Amount": {
            "en-US": "Amount: {0:N2}",
            "fr": "Montant : {0:N2}",
            "de-DE": "Betrag: {0:N2}"
          }
        }
        """;

    [Fact]
    public async Task PublicContexts_SharingACatalogRetainIndependentConcurrentSelections()
    {
        var catalog = LocalizationCatalog.FromJson(CatalogJson);
        var first = new LocalizationContext(catalog);
        var second = new LocalizationContext(catalog, "fr-FR");
        var firstEvents = 0;
        var secondEvents = 0;
        first.Changed += (_, _) => Interlocked.Increment(ref firstEvents);
        second.Changed += (_, _) => Interlocked.Increment(ref secondEvents);

        await Task.WhenAll(
            Task.Run(() =>
            {
                for (var index = 0; index < 1_000; index++)
                {
                    var german = index % 2 == 0;
                    first.SetCulture(german ? "de-DE" : "en-US");
                    Assert.Equal(german ? "Hallo" : "Hello", first.Parse("Key.Greeting"));
                }
            }),
            Task.Run(() =>
            {
                for (var index = 0; index < 1_000; index++)
                {
                    second.SetCulture(index % 2 == 0 ? "fr-CA" : "fr-FR");
                    Assert.Equal("Bonjour", second.Parse("Greeting"));
                }
            })
        );

        Assert.Equal(1_000, firstEvents);
        Assert.Equal(1_000, secondEvents);
        Assert.Equal("en-US", first.Culture);
        Assert.Equal("fr-FR", second.Culture);
        first.SetCulture("de-DE");
        Assert.Equal("Bonjour", second.Parse("Key.Greeting"));
        Assert.Equal(1_000, secondEvents);
    }

    [Fact]
    public void FormattingCulture_IsIndependentReadOnlyAndDoesNotAlterAmbientState()
    {
        var ambient = CultureInfo.CurrentCulture;
        var ambientUi = CultureInfo.CurrentUICulture;
        var defaultCulture = CultureInfo.DefaultThreadCurrentCulture;
        var defaultUi = CultureInfo.DefaultThreadCurrentUICulture;
        var context = new LocalizationContext(
            LocalizationCatalog.FromJson(CatalogJson), "fr-CA", "de_DE"
        );

        Assert.Equal("fr-CA", context.Culture);
        Assert.Equal("de-DE", context.FormatCulture.Name);
        Assert.True(context.FormatCulture.IsReadOnly);
        Assert.Equal("Montant : 1.234,50", context.Parse("Key.Amount", 1234.5m));
        Assert.Throws<InvalidOperationException>(() => context.FormatCulture.NumberFormat = new());

        context.SetCulture("en-US", "de-DE");
        Assert.Equal("Amount: 1.234,50", context.Parse("Key.Amount", 1234.5m));
        Assert.Same(ambient, CultureInfo.CurrentCulture);
        Assert.Same(ambientUi, CultureInfo.CurrentUICulture);
        Assert.Same(defaultCulture, CultureInfo.DefaultThreadCurrentCulture);
        Assert.Same(defaultUi, CultureInfo.DefaultThreadCurrentUICulture);
    }

    [Fact]
    public async Task ConcurrentReadAndSwitch_NeverMixTranslationAndFormattingSnapshots()
    {
        var context = new LocalizationContext(
            LocalizationCatalog.FromJson(CatalogJson), "en-US", "de-DE"
        );
        var expected = new[] { "Amount: 1.234,50", "Betrag: 1,234.50" };

        await Task.WhenAll(
            Task.Run(() =>
            {
                for (var index = 0; index < 2_000; index++)
                {
                    Assert.Contains(context.Parse("Key.Amount", 1234.5m), expected);
                }
            }),
            Task.Run(() =>
            {
                for (var index = 0; index < 1_000; index++)
                {
                    if (index % 2 == 0)
                    {
                        context.SetCulture("de-DE", "en-US");
                    }
                    else
                    {
                        context.SetCulture("en-US", "de-DE");
                    }
                }
            })
        );
    }

    [Fact]
    public void Changed_CommitsThePairAndNormalizesNoOpSelections()
    {
        var context = new LocalizationContext(LocalizationCatalog.FromJson(CatalogJson));
        var observed = new List<(string Culture, string Format, string Text)>();
        context.Changed += (sender, _) =>
        {
            Assert.Same(context, sender);
            observed.Add((context.Culture, context.FormatCulture.Name, context.Parse("Greeting")));
        };

        context.SetCulture("de_DE", "en_US");
        context.SetCulture("de-DE", "en-US");
        context.SetCulture("de-DE");
        context.SetCulture("de_DE", "de_DE");

        Assert.Equal(
            new[]
            {
                ("de-DE", "en-US", "Hallo"),
                ("de-DE", "de-DE", "Hallo"),
            },
            observed
        );
    }

    [Fact]
    public void Load_ReadsTheCurrentStreamPositionAndLeavesTheCallerStreamOpen()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("skip" + CatalogJson));
        stream.Position = 4;
        var catalog = LocalizationCatalog.Load(stream);

        Assert.True(stream.CanRead);
        Assert.Equal("Hello", new LocalizationContext(catalog).Parse("Greeting"));
        Assert.Equal("en-US", catalog.FallbackCulture);
        Assert.Equal(new[] { "de-DE", "en-US", "fr" }, catalog.AvailableCultures);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<string>)catalog.AvailableCultures)[0] = "changed"
        );
    }

    [Fact]
    public void Load_LeavesCallerStreamOpenAfterValidationFailure()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("{}"));
        Assert.Throws<InvalidDataException>(() => LocalizationCatalog.Load(stream));
        Assert.True(stream.CanRead);
    }

    [Theory]
    [InlineData("not-json")]
    [InlineData("{}")]
    [InlineData("[]")]
    [InlineData("{\"Value\":{\"en-US\":\"Value\"},\"Other\":{\"fr\":\"Autre\"}}")]
    [InlineData("{\"Value\":{\"en-US\":\"Value\",\"fr\":\"Valeur\"},\"Other\":{\"en-US\":\"Other\"}}")]
    [InlineData("{\"Value\":{\"en-US\":\"Value\",\"en_US\":\"Duplicate\"}}")]
    [InlineData("{\"Value\":{\"en-US\":\"{0}\",\"fr\":\"{1}\"}}")]
    [InlineData("{\"Value\":{\"en-US\":\"broken {0\"}}")]
    [InlineData("{\"Value.Illegal\":{\"en-US\":\"Value\"}}")]
    [InlineData("{\"Value\":{\"en-US\":\" \"}}")]
    [InlineData("{\"Value\":{\"en-US\":\"Value\"},\"Value\":{\"en-US\":\"Again\"}}")]
    public void PublicCatalog_RejectsTheExistingInvalidSchema(string json) =>
        Assert.Throws<InvalidDataException>(() => LocalizationCatalog.FromJson(json));

    [Fact]
    public void ParentAndConfiguredFallback_PreserveCustomCultureBehavior()
    {
        var parent = new LocalizationContext(LocalizationCatalog.FromJson(CatalogJson), "fr-CA");
        Assert.Equal("Bonjour", parent.Parse("Key.Greeting"));

        var catalog = LocalizationCatalog.FromJson(
            "{\"Value\":{\"de-DE\":\"Wert {0:N2}\"}}", "de_DE"
        );
        var fallback = new LocalizationContext(catalog, "x-demo", "en-US");
        Assert.Equal("Wert 1,234.50", fallback.Parse("Key.Value", 1234.5m));
        fallback.SetCulture("x-demo");
        Assert.Same(CultureInfo.InvariantCulture, fallback.FormatCulture);
    }

    [Fact]
    public void UnknownTokensAndMalformedValues_PreserveLookupAndFormattingSemantics()
    {
        var context = new LocalizationContext(LocalizationCatalog.FromJson(CatalogJson));
        Assert.True(context.Contains("Greeting"));
        Assert.True(context.Contains("Key.Greeting"));
        Assert.False(context.Contains("Key.Missing"));
        Assert.Equal("Key.Missing", context.Parse("Key.Missing"));
        Assert.Equal("Missing", context.Parse("Missing"));
        Assert.False(context.TryParse("Key.Missing", out var missing));
        Assert.Equal(string.Empty, missing);
        Assert.False(context.TryParse("Key.invalid.name", out _));
        Assert.Throws<ArgumentException>(() => context.Parse("Key.invalid.name"));
        Assert.Throws<ArgumentNullException>(() => context.Parse("Greeting", (object?[])null!));

        var formatted = new LocalizationContext(LocalizationCatalog.FromJson(
            "{\"Pair\":{\"en-US\":\"{0} {1}\"}}"
        ));
        Assert.Throws<FormatException>(() => formatted.TryParse("Pair", new object?[] { 1 }, out _));
        Assert.Equal("{0} {1}", formatted.Parse("Pair"));
    }

    [Fact]
    public void GenericAndArrayOverloads_ShareTheExplicitCulture()
    {
        var context = new LocalizationContext(LocalizationCatalog.FromJson("""
            {
              "One": { "en-US": "{0:N2}" },
              "Two": { "en-US": "{0:N2}/{1:N2}" },
              "Three": { "en-US": "{0:N2}/{1:N2}/{2:N2}" }
            }
            """), "en-US", "de-DE");

        Assert.Equal("1,50", context.Parse("One", 1.5m));
        Assert.Equal("1,50/2,50", context.Parse("Two", 1.5m, 2.5m));
        Assert.Equal("1,50/2,50/3,50", context.Parse("Three", 1.5m, 2.5m, 3.5m));
        Assert.True(context.TryParse("One", 1.5m, out var one));
        Assert.Equal("1,50", one);
        Assert.True(context.TryParse("Two", 1.5m, 2.5m, out var two));
        Assert.Equal("1,50/2,50", two);
        Assert.True(context.TryParse("Three", 1.5m, 2.5m, 3.5m, out var three));
        Assert.Equal("1,50/2,50/3,50", three);
        Assert.True(context.TryParse("Three", new object?[] { 1.5m, 2.5m, 3.5m }, out var array));
        Assert.Equal(three, array);
        Assert.Equal(three, context.Parse("Three", new object?[] { 1.5m, 2.5m, 3.5m }));
    }

    [Fact]
    public void ContextSelections_DoNotMutateTheExistingStaticFacade()
    {
        var previous = Localizer.Current.Culture;
        var context = new LocalizationContext(LocalizationCatalog.FromJson(CatalogJson), "de-DE");
        context.SetCulture("fr-CA", "de-DE");
        Assert.Equal(previous, Localizer.Current.Culture);
    }

    [Fact]
    public void InvalidSelections_LeaveTheCommittedPairUnchanged()
    {
        var context = new LocalizationContext(LocalizationCatalog.FromJson(CatalogJson));
        var notifications = 0;
        context.Changed += (_, _) => notifications++;
        Assert.Throws<ArgumentException>(() => context.SetCulture("invalid!", "de-DE"));
        Assert.Throws<ArgumentException>(() => context.SetCulture("de-DE", "invalid!"));
        Assert.Equal("en-US", context.Culture);
        Assert.Equal("en-US", context.FormatCulture.Name);
        Assert.Equal(0, notifications);
    }

    [Theory]
    [InlineData(" zh_CN ", "zh-CN")]
    [InlineData("x-demo", "x-Demo")]
    [InlineData("abcdefghij", "abcdefghij")]
    public void CultureNames_UseTheSameNormalizationForKnownAndCustomTags(
        string input, string expected
    ) => Assert.Equal(expected, CultureName.Normalize(input));


}
