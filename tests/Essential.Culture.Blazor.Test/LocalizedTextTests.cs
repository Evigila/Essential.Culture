using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ArkheideSystem.Essential.Culture.Blazor.Test;

public sealed class LocalizedTextTests
{
    [Fact]
    public async Task ServerRenderedTextUpdatesOnlyWithinItsScope()
    {
        await using var provider = LocalizationServiceTests.Provider();
        using var first = provider.CreateScope();
        using var second = provider.CreateScope();
        await using var rendererA = new HtmlRenderer(first.ServiceProvider, provider.GetRequiredService<ILoggerFactory>());
        await using var rendererB = new HtmlRenderer(second.ServiceProvider, provider.GetRequiredService<ILoggerFactory>());
        var rootA = await rendererA.Dispatcher.InvokeAsync(() => rendererA.RenderComponentAsync<LocalizedText>(Parameters("Greeting")));
        var rootB = await rendererB.Dispatcher.InvokeAsync(() => rendererB.RenderComponentAsync<LocalizedText>(Parameters("Greeting")));
        Assert.Equal("Hello", await rendererA.Dispatcher.InvokeAsync(rootA.ToHtmlString));
        await rendererA.Dispatcher.InvokeAsync(() => first.ServiceProvider.GetRequiredService<ILocalizationService>().SetCulture("zh-CN"));
        var updated = await rendererA.Dispatcher.InvokeAsync(rootA.ToHtmlString);
        Assert.Contains("你好", System.Net.WebUtility.HtmlDecode(updated));
        Assert.Equal("Hello", await rendererB.Dispatcher.InvokeAsync(rootB.ToHtmlString));
    }

    [Fact]
    public async Task ComponentEncodesTranslationsAndUsesLiteralFallback()
    {
        await using var provider = LocalizationServiceTests.Provider(options => options.AddCatalog("Markup", LocalizationServiceTests.Catalog("<script>alert(1)</script>", "文字")));
        using var scope = provider.CreateScope();
        await using var renderer = new HtmlRenderer(scope.ServiceProvider, provider.GetRequiredService<ILoggerFactory>());
        var parameters = ParameterView.FromDictionary(new Dictionary<string, object?> { ["Key"] = "Greeting", ["CatalogId"] = "Markup" });
        var root = await renderer.Dispatcher.InvokeAsync(() => renderer.RenderComponentAsync<LocalizedText>(parameters));
        var html = await renderer.Dispatcher.InvokeAsync(root.ToHtmlString);
        Assert.DoesNotContain("<script>", html);
        Assert.Contains("&lt;script&gt;", html);
        var fallback = await renderer.Dispatcher.InvokeAsync(() => renderer.RenderComponentAsync<LocalizedText>(ParameterView.FromDictionary(
            new Dictionary<string, object?> { ["Key"] = "Missing", ["Fallback"] = "<fallback>" })));
        Assert.Equal("&lt;fallback&gt;", await renderer.Dispatcher.InvokeAsync(fallback.ToHtmlString));
    }

    [Fact]
    public async Task FormattingAndCatalogSelectionWorkInRazorRendering()
    {
        await using var provider = LocalizationServiceTests.Provider();
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ILocalizationService>();
        service.SetCulture("zh-CN", "de-DE");
        await using var renderer = new HtmlRenderer(scope.ServiceProvider, provider.GetRequiredService<ILoggerFactory>());
        var root = await renderer.Dispatcher.InvokeAsync(() => renderer.RenderComponentAsync<LocalizedText>(ParameterView.FromDictionary(
            new Dictionary<string, object?> { ["Key"] = "Amount", ["CatalogId"] = "App", ["Arguments"] = new object?[] { 1234.5 } })));
        Assert.Equal("金额：1.234,50", System.Net.WebUtility.HtmlDecode(await renderer.Dispatcher.InvokeAsync(root.ToHtmlString)));
    }

    [Fact]
    public async Task RendererDisposalRemovesTheComponentSubscription()
    {
        await using var provider = LocalizationServiceTests.Provider();
        using var scope = provider.CreateScope();
        var tracking = new TrackingService(scope.ServiceProvider.GetRequiredService<ILocalizationService>());
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<ILocalizationService>(tracking);
        await using var renderingProvider = services.BuildServiceProvider();
        var renderer = new HtmlRenderer(renderingProvider, renderingProvider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(() => renderer.RenderComponentAsync<LocalizedText>(Parameters("Greeting")));
        Assert.Equal(1, tracking.SubscriptionCount);
        await renderer.DisposeAsync();
        Assert.Equal(0, tracking.SubscriptionCount);
        tracking.SetCulture("zh-CN");
        Assert.Equal("你好", tracking.Parse("Greeting"));
    }

    [Fact]
    public async Task BackgroundNotificationIsMarshaledToTheRenderer()
    {
        await using var provider = LocalizationServiceTests.Provider();
        using var scope = provider.CreateScope();
        await using var renderer = new HtmlRenderer(scope.ServiceProvider, provider.GetRequiredService<ILoggerFactory>());
        var root = await renderer.Dispatcher.InvokeAsync(() => renderer.RenderComponentAsync<LocalizedText>(Parameters("Greeting")));
        await Task.Run(() => scope.ServiceProvider.GetRequiredService<ILocalizationService>().SetCulture("zh-CN"));
        Assert.Equal("你好", System.Net.WebUtility.HtmlDecode(await renderer.Dispatcher.InvokeAsync(root.ToHtmlString)));
    }

    [Fact]
    public async Task QueuedBackgroundRefreshDoesNotRenderAfterDisposal()
    {
        await using var provider = LocalizationServiceTests.Provider();
        using var scope = provider.CreateScope();
        var tracking = new TrackingService(scope.ServiceProvider.GetRequiredService<ILocalizationService>());
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<ILocalizationService>(tracking);
        await using var renderingProvider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(renderingProvider, renderingProvider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(() => renderer.RenderComponentAsync<LocalizedText>(Parameters("Greeting")));
        await renderer.Dispatcher.InvokeAsync(() =>
        {
            // Hold the renderer while the event queues a refresh, then dispose before it can run.
            Task.Run(() => tracking.SetCulture("zh-CN")).GetAwaiter().GetResult();
            renderer.DisposeAsync().GetAwaiter().GetResult();
        });
        await renderer.Dispatcher.InvokeAsync(() => { });
        Assert.Equal(0, tracking.SubscriptionCount);
    }

    private static ParameterView Parameters(string key) => ParameterView.FromDictionary(new Dictionary<string, object?> { ["Key"] = key });

    private sealed class TrackingService(ILocalizationService inner) : ILocalizationService
    {
        public int SubscriptionCount { get; private set; }
        public string Culture => inner.Culture;
        public System.Globalization.CultureInfo FormatCulture => inner.FormatCulture;
        public IReadOnlyList<string> AvailableCultures => inner.AvailableCultures;
        public event EventHandler? Changed
        {
            add { SubscriptionCount++; inner.Changed += value; }
            remove { SubscriptionCount--; inner.Changed -= value; }
        }
        public void SetCulture(string culture, string? formatCulture = null) => inner.SetCulture(culture, formatCulture);
        public string Parse(string token, params object?[] arguments) => inner.Parse(token, arguments);
        public string ParseFrom(string id, string token, params object?[] arguments) => inner.ParseFrom(id, token, arguments);
        public bool TryParse(string token, out string value) => inner.TryParse(token, out value);
        public bool TryParse(string token, object?[] arguments, out string value) => inner.TryParse(token, arguments, out value);
        public bool TryParseFrom(string id, string token, out string value) => inner.TryParseFrom(id, token, out value);
        public bool TryParseFrom(string id, string token, object?[] arguments, out string value) => inner.TryParseFrom(id, token, arguments, out value);
        public bool Contains(string token) => inner.Contains(token);
        public bool ContainsFrom(string id, string token) => inner.ContainsFrom(id, token);
    }
}
