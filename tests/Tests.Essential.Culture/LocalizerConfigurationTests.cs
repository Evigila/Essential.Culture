using System.Reflection;
using System.Runtime.Loader;

namespace ArkheideSystem.Essential.Culture.Test;

public sealed class LocalizerConfigurationTests
{
    [Fact]
    public void ConfigureUsesAnExplicitCatalogBeforeAnyDefaultFileInitialization()
    {
        using var isolated = new IsolatedFacade();
        var catalog = isolated.Catalog("""{"OnlyConfigured":{"fr":"Bonjour"}}""", "fr");
        var previousDirectory = AppContext.GetData("APP_CONTEXT_BASE_DIRECTORY");
        try
        {
            AppContext.SetData("APP_CONTEXT_BASE_DIRECTORY", Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));
            isolated.Configure(catalog, "fr", "de-DE");
            Assert.Equal("Bonjour", isolated.Parse("Key.OnlyConfigured"));
        }
        finally { AppContext.SetData("APP_CONTEXT_BASE_DIRECTORY", previousDirectory); }
        var error = Assert.Throws<TargetInvocationException>(() => isolated.Configure(catalog, "fr", null));
        Assert.IsType<InvalidOperationException>(error.InnerException);
    }

    [Fact]
    public void FacadeEventSubscriptionPreventsLaterConfiguration()
    {
        using var isolated = new IsolatedFacade();
        EventHandler handler = (_, _) => { };
        var changed = isolated.CurrentType.GetEvent("Changed")!;
        changed.AddEventHandler(null, handler);
        try
        {
            var catalog = isolated.Catalog("""{"Other":{"en-US":"Other"}}""", "en-US");
            var error = Assert.Throws<TargetInvocationException>(() => isolated.Configure(catalog, "en-US", null));
            Assert.IsType<InvalidOperationException>(error.InnerException);
        }
        finally { changed.RemoveEventHandler(null, handler); }
    }

    [Fact]
    public void InvalidConfigurationDoesNotConsumeTheOneTimeConfigurationOpportunity()
    {
        using var isolated = new IsolatedFacade();
        var catalog = isolated.Catalog("""{"Greeting":{"en-US":"Hello"}}""", "en-US");
        var error = Assert.Throws<TargetInvocationException>(() => isolated.Configure(catalog, "invalid!", null));
        Assert.IsType<ArgumentException>(error.InnerException);
        isolated.Configure(catalog, "en-US", null);
        Assert.Equal("Hello", isolated.Parse("Greeting"));
    }

    [Fact]
    public void FailedDefaultInitializationStillPreventsConfigurationAndKeepsTheLegacyLazyFault()
    {
        using var isolated = new IsolatedFacade();
        var previousDirectory = AppContext.GetData("APP_CONTEXT_BASE_DIRECTORY");
        var directory = Path.Combine(Path.GetTempPath(), "culture-configure-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            AppContext.SetData("APP_CONTEXT_BASE_DIRECTORY", directory);
            var failure = Assert.Throws<TargetInvocationException>(() => isolated.Parse("Greeting"));
            Assert.IsType<FileNotFoundException>(Assert.IsType<TypeInitializationException>(failure.InnerException).InnerException);
        }
        finally
        {
            AppContext.SetData("APP_CONTEXT_BASE_DIRECTORY", previousDirectory);
            Directory.Delete(directory);
        }
        var catalog = isolated.Catalog("""{"Greeting":{"en-US":"Hello"}}""", "en-US");
        var configureFailure = Assert.Throws<TargetInvocationException>(() => isolated.Configure(catalog, "en-US", null));
        Assert.IsType<InvalidOperationException>(configureFailure.InnerException);
        var repeatedFailure = Assert.Throws<TargetInvocationException>(() => isolated.Parse("Greeting"));
        Assert.IsType<TypeInitializationException>(repeatedFailure.InnerException);
    }

    [Fact]
    public async Task ConcurrentConfigurationsPublishExactlyOneCompleteFacade()
    {
        using var isolated = new IsolatedFacade();
        var catalog = isolated.Catalog("""{"Greeting":{"en-US":"Hello","fr":"Bonjour"}}""", "en-US");
        var attempts = await Task.WhenAll(new[] { "en-US", "fr" }.Select(culture => Task.Run(() =>
        {
            try { isolated.Configure(catalog, culture, null); return culture; }
            catch (TargetInvocationException error) when (error.InnerException is InvalidOperationException) { return null; }
        })));
        var chosen = Assert.Single(attempts, value => value is not null);
        Assert.Equal(chosen == "fr" ? "Bonjour" : "Hello", isolated.Parse("Greeting"));
    }

    private sealed class IsolatedFacade : IDisposable
    {
        private readonly AssemblyLoadContext loadContext = new("culture-configure-test", isCollectible: true);
        private readonly Type catalogType;
        private readonly Type localizerType;

        internal IsolatedFacade()
        {
            var assembly = loadContext.LoadFromAssemblyPath(typeof(Localizer).Assembly.Location);
            catalogType = assembly.GetType(typeof(LocalizationCatalog).FullName!)!;
            localizerType = assembly.GetType(typeof(Localizer).FullName!)!;
            CurrentType = localizerType.GetNestedType("Current")!;
        }

        internal Type CurrentType { get; }
        internal object Catalog(string json, string fallback) =>
            catalogType.GetMethod("FromJson", [typeof(string), typeof(string)])!.Invoke(null, [json, fallback])!;
        internal void Configure(object catalog, string culture, string? formatCulture) =>
            localizerType.GetMethod("Configure")!.Invoke(null, [catalog, culture, formatCulture]);
        internal string Parse(string token) =>
            (string)localizerType.GetMethod("Parse", [typeof(string)])!.Invoke(null, [token])!;
        public void Dispose() => loadContext.Unload();
    }
}
