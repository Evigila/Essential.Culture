using Microsoft.Extensions.DependencyInjection;

namespace ArkheideSystem.Essential.Culture.Blazor.Test;

public sealed class LanguagePolicyTests
{
    private const string Document = """
        {"Greeting":{"en-US":"Hello","fr":"Bonjour","fr-CA":"Salut"}}
        """;

    [Fact]
    public void HostCannotReenableCatalogDeniedCulture()
    {
        var catalog = LocalizationCatalog.FromJson(Document, "en-US",
            new CatalogLoadOptions(disabledCultures: ["fr_CA"]));
        var services = new ServiceCollection();
        var error = Assert.Throws<InvalidOperationException>(() => services.AddCultureBlazor(builder =>
            builder.AddCatalog("App", catalog).AddSupportedCultures("en-US", "fr-CA")));
        Assert.Contains("App", error.Message);
        Assert.Contains("fr-CA", error.Message);
    }

    [Fact]
    public void DefaultSupportedListAndDirectSelectionHonorPolicy()
    {
        var catalog = LocalizationCatalog.FromJson(Document, "en-US",
            new CatalogLoadOptions(disabledCultures: ["fr-CA"]));
        var services = new ServiceCollection();
        services.AddCultureBlazor(builder => builder.AddCatalog("App", catalog)
            .InitializeWith(_ => new LocalizationSelection("en-US")));
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ILocalizationService>();
        Assert.DoesNotContain("fr-CA", service.AvailableCultures);
        var changed = 0;
        service.Changed += (_, _) => changed++;
        Assert.Throws<ArgumentException>(() => service.SetCulture("FR_ca"));
        Assert.Equal("en-US", service.Culture);
        Assert.Equal("Hello", service.Parse("Greeting"));
        Assert.Equal(0, changed);
        service.SetCulture("fr", "fr-CA");
        Assert.Equal("Bonjour", service.Parse("Greeting"));
        Assert.Equal("fr-CA", service.FormatCulture.Name);
    }

    [Fact]
    public void CustomAllowlistRequestUsesPermittedParentAndRemainsScoped()
    {
        var catalog = LocalizationCatalog.FromJson("""{"Greeting":{"en-US":"Hello","fr":"Bonjour"}}""",
            "en-US", new CatalogLoadOptions(enabledCultures: ["en-US", "fr-CA"]));
        var services = new ServiceCollection();
        services.AddCultureBlazor(builder => builder.AddCatalog("App", catalog)
            .InitializeWith(_ => new LocalizationSelection("en-US")));
        using var provider = services.BuildServiceProvider();
        using var firstScope = provider.CreateScope();
        using var secondScope = provider.CreateScope();
        var first = firstScope.ServiceProvider.GetRequiredService<ILocalizationService>();
        var second = secondScope.ServiceProvider.GetRequiredService<ILocalizationService>();
        first.SetCulture("fr-CA");
        Assert.Equal("Bonjour", first.Parse("Greeting"));
        Assert.Equal("Hello", second.Parse("Greeting"));
        Assert.Throws<ArgumentException>(() => first.SetCulture("fr"));
    }

    [Fact]
    public void ConflictingSecondaryCatalogFailsDuringRegistration()
    {
        var main = LocalizationCatalog.FromJson(Document);
        var secondary = LocalizationCatalog.FromJson(Document, "en-US",
            new CatalogLoadOptions(disabledCultures: ["fr"]));
        Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddCultureBlazor(builder =>
            builder.AddCatalog("App", main).AddCatalog("Library", secondary)));
    }

    [Fact]
    public void InitializerCannotBypassLanguagePolicy()
    {
        var catalog = LocalizationCatalog.FromJson(Document, "en-US",
            new CatalogLoadOptions(enabledCultures: ["en-US"]));
        var services = new ServiceCollection();
        services.AddCultureBlazor(builder => builder.AddCatalog("App", catalog)
            .InitializeWith(_ => new LocalizationSelection("fr")));
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        Assert.Throws<ArgumentException>(() => scope.ServiceProvider.GetRequiredService<ILocalizationService>());
    }
}
