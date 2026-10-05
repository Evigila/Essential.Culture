using System.Globalization;
using Microsoft.Extensions.DependencyInjection;

namespace ArkheideSystem.Essential.Culture.Blazor.Test;

public sealed class LocalizationServiceTests
{
    internal static LocalizationCatalog Catalog(string english = "Hello", string chinese = "你好") =>
        LocalizationCatalog.FromJson(System.Text.Json.JsonSerializer.Serialize(
            new Dictionary<string, Dictionary<string, string>>
            {
                ["Greeting"] = new() { ["en-US"] = english, ["zh-CN"] = chinese },
                ["Amount"] = new() { ["en-US"] = "Amount: {0:N2}", ["zh-CN"] = "金额：{0:N2}" }
            }));

    internal static ServiceProvider Provider(Action<LocalizationBuilder>? additional = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCultureBlazor(options =>
        {
            options.AddCatalog("App", Catalog()).SetDefaultCulture("en-US")
                .InitializeWith(_ => new LocalizationSelection("en-US"));
            additional?.Invoke(options);
        });
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }

    [Fact]
    public void LanguageAndEventsBelongToOneScope()
    {
        using var provider = Provider();
        using var a = provider.CreateScope();
        using var b = provider.CreateScope();
        var first = a.ServiceProvider.GetRequiredService<ILocalizationService>();
        var second = b.ServiceProvider.GetRequiredService<ILocalizationService>();
        Assert.Same(first, a.ServiceProvider.GetRequiredService<ILocalizationService>());
        Assert.NotSame(first, second);
        var observed = new List<string>();
        var otherEvents = 0;
        first.Changed += (_, _) => observed.Add(first.Parse("Key.Greeting"));
        second.Changed += (_, _) => otherEvents++;
        first.SetCulture("zh_CN");
        first.SetCulture("zh-CN");
        Assert.Equal("你好", first.Parse("Greeting"));
        Assert.Equal("Hello", second.Parse("Greeting"));
        Assert.Equal(["你好"], observed);
        Assert.Equal(0, otherEvents);
    }

    [Fact]
    public async Task ConcurrentLanguageChangesDontAffectAnotherScope()
    {
        using var provider = Provider();
        using var a = provider.CreateScope();
        using var b = provider.CreateScope();
        var first = a.ServiceProvider.GetRequiredService<ILocalizationService>();
        var second = b.ServiceProvider.GetRequiredService<ILocalizationService>();
        await Task.WhenAll(Task.Run(() =>
        {
            for (var index = 0; index < 500; index++) first.SetCulture(index % 2 == 0 ? "zh-CN" : "en-US");
        }), Task.Run(() =>
        {
            for (var index = 0; index < 2_000; index++) Assert.Equal("Hello", second.Parse("Greeting"));
        }));
    }

    [Fact]
    public void SameTokenInDifferentCatalogsRemainsDistinctAcrossSwitches()
    {
        using var provider = Provider(options => options.AddCatalog("Library", Catalog("Library", "控件库")));
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ILocalizationService>();
        Assert.Equal("Hello", service.Parse("Key.Greeting"));
        Assert.Equal("Library", service.ParseFrom("Library", "Key.Greeting"));
        service.SetCulture("zh-CN");
        Assert.Equal("你好", service.Parse("Key.Greeting"));
        Assert.Equal("控件库", service.ParseFrom("Library", "Key.Greeting"));
        Assert.Throws<KeyNotFoundException>(() => service.ParseFrom("Unknown", "Greeting"));
    }

    [Fact]
    public void FormattingIsExplicitAndNeverChangesAmbientOrProcessDefaults()
    {
        var beforeUi = CultureInfo.CurrentUICulture;
        var beforeFormat = CultureInfo.CurrentCulture;
        var defaultUi = CultureInfo.DefaultThreadCurrentUICulture;
        var defaultFormat = CultureInfo.DefaultThreadCurrentCulture;
        using var provider = Provider();
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ILocalizationService>();
        service.SetCulture("zh-CN", "de-DE");
        Assert.Equal("金额：1.234,50", service.Parse("Amount", 1234.5));
        Assert.True(service.FormatCulture.IsReadOnly);
        Assert.Equal(beforeUi, CultureInfo.CurrentUICulture);
        Assert.Equal(beforeFormat, CultureInfo.CurrentCulture);
        Assert.Equal(defaultUi, CultureInfo.DefaultThreadCurrentUICulture);
        Assert.Equal(defaultFormat, CultureInfo.DefaultThreadCurrentCulture);
    }

    [Fact]
    public void FormattingOnlyChangeRaisesAnEvent()
    {
        using var provider = Provider();
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ILocalizationService>();
        var events = 0;
        service.Changed += (_, _) => events++;
        service.SetCulture("en-US", "de-DE");
        service.SetCulture("en-US", "de-DE");
        Assert.Equal(1, events);
        Assert.Equal("Amount: 1.234,50", service.Parse("Amount", 1234.5));
    }

    [Theory]
    [InlineData("fr-FR", null)]
    [InlineData("", null)]
    [InlineData("en-US", "")]
    public void InvalidSelectionLeavesStateUnchanged(string culture, string? format)
    {
        using var provider = Provider();
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ILocalizationService>();
        var events = 0;
        service.Changed += (_, _) => events++;
        Assert.ThrowsAny<ArgumentException>(() => service.SetCulture(culture, format));
        Assert.Equal("en-US", service.Culture);
        Assert.Equal(0, events);
    }

    [Fact]
    public void FactoryResolvesHostPreferencesForEachScope()
    {
        var services = new ServiceCollection();
        services.AddScoped<Preference>();
        services.AddCultureBlazor(options => options.AddCatalog("App", Catalog())
            .InitializeWith(provider => new LocalizationSelection(provider.GetRequiredService<Preference>().Culture)));
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        using var a = provider.CreateScope();
        using var b = provider.CreateScope();
        a.ServiceProvider.GetRequiredService<Preference>().Culture = "zh-CN";
        Assert.Equal("你好", a.ServiceProvider.GetRequiredService<ILocalizationService>().Parse("Greeting"));
        Assert.Equal("Hello", b.ServiceProvider.GetRequiredService<ILocalizationService>().Parse("Greeting"));
    }

    [Fact]
    public void AmbientRequestCulturesInitializeNewScopes()
    {
        var beforeUi = CultureInfo.CurrentUICulture;
        var beforeFormat = CultureInfo.CurrentCulture;
        try
        {
            var services = new ServiceCollection();
            services.AddCultureBlazor(options => options.AddCatalog("App", Catalog()));
            using var provider = services.BuildServiceProvider();
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("zh-CN");
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            using var scope = provider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ILocalizationService>();
            Assert.Equal("zh-CN", service.Culture);
            Assert.Equal("金额：1.234,50", service.Parse("Amount", 1234.5));
        }
        finally { CultureInfo.CurrentUICulture = beforeUi; CultureInfo.CurrentCulture = beforeFormat; }
    }

    [Fact]
    public void UnknownAmbientLanguageUsesConfiguredUiFallback()
    {
        var beforeUi = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("fr-FR");
            var services = new ServiceCollection();
            services.AddCultureBlazor(options => options.AddCatalog("App", Catalog()));
            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            Assert.Equal("en-US", scope.ServiceProvider.GetRequiredService<ILocalizationService>().Culture);
        }
        finally { CultureInfo.CurrentUICulture = beforeUi; }
    }

    [Fact]
    public void TryParseAndMissingTokenBehaviorMatchesCore()
    {
        using var provider = Provider();
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ILocalizationService>();
        Assert.True(service.TryParse("Greeting", out var text));
        Assert.Equal("Hello", text);
        Assert.True(service.ContainsFrom("App", "Key.Greeting"));
        Assert.False(service.TryParse("bad key", out text));
        Assert.Equal(string.Empty, text);
        Assert.False(service.Contains("Missing"));
        Assert.Equal("Key.Missing", service.Parse("Key.Missing"));
        Assert.Throws<ArgumentNullException>(() => service.TryParse("Amount", null!, out _));
    }

    [Fact]
    public void CapturedBuilderAndDuplicateRegistrationsCannotMutateCompletedOptions()
    {
        LocalizationBuilder? captured = null;
        var services = new ServiceCollection();
        services.AddCultureBlazor(options => { captured = options; options.AddCatalog("App", Catalog()); });
        Assert.Throws<InvalidOperationException>(() => captured!.SetDefaultCulture("zh-CN"));
        Assert.Throws<InvalidOperationException>(() => services.AddCultureBlazor(options => options.AddCatalog("App", Catalog())));
        Assert.Throws<ArgumentException>(() => new ServiceCollection().AddCultureBlazor(options => options
            .AddCatalog("App", Catalog()).AddCatalog("App", Catalog())));
        Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddCultureBlazor(_ => { }));
    }

    [Fact]
    public void PrivateFormattingTagUsesCoreInvariantFallbackAndRemainsIdempotent()
    {
        using var provider = Provider();
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ILocalizationService>();
        var events = 0;
        service.Changed += (_, _) => events++;
        service.SetCulture("en-US", "x-demo");
        service.SetCulture("en-US", "x-DEMO");
        Assert.Equal(1, events);
        Assert.Equal(CultureInfo.InvariantCulture, service.FormatCulture);
        Assert.Equal("Amount: 1.50", service.Parse("Amount", 1.5));
    }

    [Fact]
    public void ExplicitlyAllowedPrivateUiTagUsesCatalogFallback()
    {
        using var provider = Provider(options => options.AddSupportedCultures("en-US", "zh-CN", "x-demo"));
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ILocalizationService>();
        service.SetCulture("x-demo");
        Assert.Equal("x-Demo", service.Culture);
        Assert.Equal("Hello", service.Parse("Greeting"));
    }

    private sealed class Preference { public string Culture { get; set; } = "en-US"; }
}
