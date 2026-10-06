using ArkheideSystem.Essential.Culture;
using ArkheideSystem.Essential.Culture.Blazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PackageConsumer.Texts;

// Only Blazor is referenced directly: Key comes from the transitive Generator,
// and LocalizationCatalog comes from transitive Core.
var catalog = LocalizationCatalog.FromFile(Path.Combine(AppContext.BaseDirectory, "Culture.json"));
var services = new ServiceCollection();
services.AddLogging();
services.AddCultureBlazor(options => options.AddCatalog("App", catalog)
    .SetDefaultCulture("en-US").InitializeWith(_ => new LocalizationSelection("en-US")));
await using var provider = services.BuildServiceProvider();
using var first = provider.CreateScope();
using var second = provider.CreateScope();
var firstService = first.ServiceProvider.GetRequiredService<ILocalizationService>();
var secondService = second.ServiceProvider.GetRequiredService<ILocalizationService>();
Assert(firstService.Parse(Key.Greeting) == "Hello", "Generated keys resolve through transitive Core.");
firstService.SetCulture("zh-CN", "de-DE");
Assert(firstService.Parse(Key.Greeting) == "你好", "The current scope switches language.");
Assert(secondService.Parse(Key.Greeting) == "Hello", "Another scope keeps its language.");
Assert(firstService.TryParse(Key.Amount, [1234.5], out var formatted) && formatted == "金额 1.234,50",
    "Formatting uses the explicit format culture.");
await using var renderer = new HtmlRenderer(first.ServiceProvider, provider.GetRequiredService<ILoggerFactory>());
var root = await renderer.Dispatcher.InvokeAsync(() => renderer.RenderComponentAsync<LocalizedText>(
    ParameterView.FromDictionary(new Dictionary<string, object?> { ["Key"] = Key.Markup })));
var html = await renderer.Dispatcher.InvokeAsync(root.ToHtmlString);
Assert(!html.Contains("<script>", StringComparison.Ordinal) && html.Contains("&lt;script&gt;", StringComparison.Ordinal),
    "Packaged LocalizedText encodes its output.");
Console.WriteLine("Blazor-only NuGet consumer passed: transitive Core and Generator, scoped language, formatting, and encoded rendering.");

static void Assert(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}
