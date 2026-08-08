using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ArkheideSystem.LangKey.Test;

public sealed class LangKeyRegistrationTests
{
    [Fact]
    public void AddLangKey_RegistersOneSharedFrameworkIndependentRuntime()
    {
        var services = new ServiceCollection();
        services.AddLangKey(GetDocumentPath(), _ => "zh-CN");

        using var provider = services.BuildServiceProvider();
        var parser = provider.GetRequiredService<ILangKeyParser>();
        var resolver = provider.GetRequiredService<ILangKeyResolver>();

        Assert.Same(parser, resolver);
        Assert.Equal("zh-CN", resolver.Current);
        Assert.Equal("你好 世界！", resolver.Get("LangKey.Title_Hello"));
        Assert.Empty(provider.GetServices<IHostedService>());
    }

    [Fact]
    public void AddLangKey_RejectsDuplicateRegistration()
    {
        var services = new ServiceCollection();
        services.AddLangKey(GetDocumentPath());

        Assert.Throws<InvalidOperationException>(() => services.AddLangKey(GetDocumentPath()));
    }

    [Fact]
    public void AddLangKey_WithCultureSourceSynchronizesWithoutFrameworkDependency()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new TestCultureSource("en-US"));
        services.AddLangKey<TestCultureSource>(GetDocumentPath());

        using var provider = services.BuildServiceProvider();
        var source = provider.GetRequiredService<TestCultureSource>();
        var parser = provider.GetRequiredService<ILangKeyParser>();

        source.ChangeTo("zh-CN");

        Assert.Equal("zh-CN", parser.Current);
    }

    [Fact]
    public void CoreAndDependencyInjectionAssemblies_PreserveFrameworkBoundaries()
    {
        var coreReferences = typeof(LangKeyParser)
            .Assembly.GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .ToHashSet(StringComparer.Ordinal);
        var dependencyInjectionReferences = typeof(LangKeyServiceCollectionExtensions)
            .Assembly.GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.DoesNotContain(
            coreReferences,
            name =>
                name is not null
                && (
                    name.StartsWith("Microsoft.Extensions", StringComparison.Ordinal)
                    || name.StartsWith("Presentation", StringComparison.Ordinal)
                    || name.Contains("Flourish", StringComparison.Ordinal)
                )
        );
        Assert.DoesNotContain(
            dependencyInjectionReferences,
            name =>
                name is not null
                && (
                    name == "Microsoft.Extensions.Hosting.Abstractions"
                    || name.StartsWith("Presentation", StringComparison.Ordinal)
                    || name.Contains("Flourish", StringComparison.Ordinal)
                )
        );
    }

    private static string GetDocumentPath() =>
        Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "LangKey.json")
        );

    private sealed class TestCultureSource(string currentCulture) : ILangKeyCultureSource
    {
        public string CurrentCulture { get; private set; } = currentCulture;

        public event EventHandler<LangKeyCultureChangedEventArgs>? Changed;

        public void ChangeTo(string culture)
        {
            CurrentCulture = culture;
            Changed?.Invoke(this, new LangKeyCultureChangedEventArgs(culture));
        }
    }
}
