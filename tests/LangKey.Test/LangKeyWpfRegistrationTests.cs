using System.IO;
using System.Windows;
using ArkheideSystem.LangKey.Wpf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ArkheideSystem.LangKey.Test;

public sealed class LangKeyWpfRegistrationTests
{
    [Fact]
    public async Task AddLangKeyWpf_RegistersOneSharedRuntimeAndResolvesRelativePath()
    {
        var services = new ServiceCollection();
        services.AddSingleton<TestApplication>();
        services.AddLangKeyWpf<TestApplication>(
            Path.GetRelativePath(AppContext.BaseDirectory, GetDocumentPath()),
            _ => "zh-CN"
        );

        using var provider = services.BuildServiceProvider();
        var parser = provider.GetRequiredService<ILangKeyParser>();

        Assert.Same(parser, provider.GetRequiredService<ILangKeyResolver>());
        Assert.Same(
            provider.GetRequiredService<LangKeyWpfApplicator>(),
            provider.GetRequiredService<ILangKeyWpfApplicator>()
        );
        Assert.Equal(GetDocumentPath(), Assert.IsType<LangKeyParser>(parser).SourcePath);
        Assert.Equal("zh-CN", parser.Current);
        var hostedService = Assert.Single(provider.GetServices<IHostedService>());
        await hostedService.StartAsync(CancellationToken.None);
        await hostedService.StopAsync(CancellationToken.None);
        await hostedService.StartAsync(CancellationToken.None);
        await hostedService.StopAsync(CancellationToken.None);
    }

    [Fact]
    public void AddLangKeyWpf_RejectsDuplicateRegistration()
    {
        var services = new ServiceCollection();
        services.AddLangKeyWpf<TestApplication>(GetDocumentPath());

        var error = Assert.Throws<InvalidOperationException>(() =>
            services.AddLangKeyWpf<TestApplication>(GetDocumentPath())
        );

        Assert.Contains("already registered", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AddLangKeyWpf_RejectsConflictingParserRegistration()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILangKeyParser>(new LangKeyParser(GetDocumentPath()));

        Assert.Throws<InvalidOperationException>(() =>
            services.AddLangKeyWpf<TestApplication>(GetDocumentPath())
        );
    }

    [Fact]
    public void AddLangKeyWpf_WithCultureSourceUsesGenericSynchronization()
    {
        var services = new ServiceCollection();
        services.AddSingleton<TestApplication>();
        services.AddSingleton(new TestCultureSource("en-US"));
        services.AddLangKeyWpf<TestApplication, TestCultureSource>(GetDocumentPath());

        using var provider = services.BuildServiceProvider();
        var source = provider.GetRequiredService<TestCultureSource>();
        var parser = provider.GetRequiredService<ILangKeyParser>();

        source.ChangeTo("zh-CN");

        Assert.Equal("zh-CN", parser.Current);
    }

    private static string GetDocumentPath() =>
        Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "LangKey.json")
        );

    private sealed class TestApplication : Application;

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
