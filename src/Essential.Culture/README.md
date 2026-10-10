# Essential.Culture

[English](https://github.com/Evigila/Essential/blob/master/src/Essential.Culture/README.md) | [简体中文](https://github.com/Evigila/Essential/blob/master/src/Essential.Culture/README_zh-CN.md)

`Essential.Culture` is a JSON-based localization component for .NET.

It manages multilingual localization keys in `Culture.json`, supports optional functional modules, generates strongly typed keys, and provides runtime culture switching for WPF, Avalonia, WinUI 3, and scoped Blazor services.

## Features

- `Culture.json` is the conventional JSON file name and is detected automatically.
- Source generation provides the `CultureKey` enum and a unified `Localize` XAML API for strongly typed keys and compile-time validation.
- `KeyBinding` allows collection items or runtime state to select localization keys dynamically.
- `Localizer.Parse(...)` and `Localizer.TryParse(...)` resolve localization keys and support format arguments.
- `Localizer.Current` provides runtime culture management.
- Startup language policy excludes unwanted translations from retained catalog memory while validating all authored resources.
- Optional `CultureModule` items compose multiple multilingual files into one catalog with globally unique keys.

## Quick start

The runtime, Avalonia and Blazor packages target **.NET 10**. WPF requires `net10.0-windows`; WinUI 3 requires `net10.0-windows10.0.19041.0` (minimum supported Windows version `10.0.17763.0`). The Generator is a `netstandard2.0` analyzer, not a standalone localization runtime. Blazor uses the ASP.NET Core shared framework and supports server-side rendering and Interactive Server; this package does not target standalone WebAssembly.

Install the package for your host. Each adapter includes the core runtime and Generator transitively; do not install every adapter into one application.

For WPF:

```powershell
dotnet add package Arkheide.Essential.Culture.Wpf
```

For Avalonia:

```powershell
dotnet add package Arkheide.Essential.Culture.Avalonia
```

For WinUI 3:

```powershell
dotnet add package Arkheide.Essential.Culture.WinUI
```

For server-side Blazor:

```powershell
dotnet add package Arkheide.Essential.Culture.Blazor
```

For console applications or other .NET 10 hosts:

```powershell
dotnet add package Arkheide.Essential.Culture
```

On the first build, the transitive Generator creates a missing project-root `Culture.json` and copies it to build/publish output. Existing files are never overwritten. Define your keys and translations:

```json
{
  "Greeting": {
    "en-US": "Hello, World!",
    "zh-CN": "你好，世界！"
  },
  "Welcome_User": {
    "en-US": "Hello, {0}!",
    "zh-CN": "你好，{0}！"
  }
}
```

Each file must be a nonempty JSON object. Keys are case-sensitive ASCII C# identifiers (letters, digits and underscores, with no leading digit or C# keyword); reserved generated names `Key`, `CultureKey` and `value__` are rejected. Each key in a file must declare the same normalized culture set, including `en-US` by default. Values must be nonempty strings with valid composite formats; every translation must preserve the fallback's placeholder indices and occurrence counts. Optional language sets may differ between module files.

The unchanged single-file default loads `AppContext.BaseDirectory/Culture.json` on the first `Localizer` use, with both selected and fallback culture `en-US`. It does not scan folders or load module files automatically.

## Start using it

> [!NOTE]
> If IntelliSense does not show the generated types yet, build the project once.

WPF uses `Localize`, and format arguments can use bindings directly:

```xml
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:culture="clr-namespace:ArkheideSystem.Essential.Culture">
  <StackPanel>
    <TextBlock Text="{culture:Localize Key=Greeting}" />
    <TextBlock Text="{culture:Localize Key=Welcome_User, Arg0={Binding UserName}}" />
    <TextBlock Text="{culture:Localize KeyBinding={Binding CurrentTextKey}}" />
  </StackPanel>
</Window>
```

Avalonia uses the same API with different namespace syntax:

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:culture="using:ArkheideSystem.Essential.Culture">
  <StackPanel>
    <TextBlock Text="{culture:Localize Key=Greeting}" />
    <TextBlock Text="{culture:Localize Key=Welcome_User, Arg0={Binding UserName}}" />
    <TextBlock Text="{culture:Localize KeyBinding={Binding CurrentTextKey}}" />
  </StackPanel>
</Window>
```

Use the strongly typed `Key=` property so the editor can suggest keys from the generated `CultureKey` enum. When the key comes from the DataContext or runtime state, use `KeyBinding=`.

WinUI 3 exposes dynamic arguments through attached properties on the same `Localize` type:

```xml
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:culture="using:ArkheideSystem.Essential.Culture">
  <StackPanel>
    <TextBlock Text="{culture:Localize Key=Greeting}" />
    <TextBlock Text="{culture:Localize Key=Welcome_User}"
               culture:Localize.Argument0="{x:Bind ViewModel.UserName, Mode=OneWay}" />
    <TextBlock Text="{culture:Localize}"
               culture:Localize.KeyBinding="{x:Bind ViewModel.CurrentTextKey, Mode=OneWay}" />
  </StackPanel>
</Window>
```

In WinUI 3, create a host, attach each window after `InitializeComponent`, and retain the host for those windows' lifetime:

```csharp
using ArkheideSystem.Essential.Culture.WinUI;

var localizationHost = new WinUILocalizationHost();
var window = new MainWindow();
localizationHost.Attach(window);
window.Closed += (_, _) => localizationHost.Dispose(); // Single-window application
window.Activate();
```

Attach/detach/dispose on the owning UI thread. A multi-window application should attach every window and dispose the shared host after the last window closes. The host resolves markers and refreshes supported WinUI display dependency properties; see the WinUI Gallery for dialogs and lifecycle integration.

These XAML excerpts belong in your existing window definition; supply its usual `x:Class`, DataContext or `ViewModel`. `Key` and `KeyBinding` cannot be used together. Dynamic keys must be raw key strings or generated `Key.*` tokens; the generated `CultureKey` enum belongs to the static `Key` property. WPF/Avalonia bindings and attached WinUI windows refresh localized target values on language or argument changes. Plain strings returned by `Parse` are snapshots; your own UI state must request another value or handle `Changed` when it needs refreshing.

## Blazor

In a server-side Blazor host, explicitly load the catalog once and register scoped localization before `builder.Build()`:

```csharp
using ArkheideSystem.Essential.Culture;
using Microsoft.Extensions.DependencyInjection;

var catalog = LocalizationCatalog.FromFile(
    Path.Combine(AppContext.BaseDirectory, "Culture.json"));
builder.Services.AddCultureBlazor(options => options
    .AddCatalog("App", catalog)
    .SetDefaultCulture("en-US")
    .AddSupportedCultures("en-US", "zh-CN"));
```

In a host already configured for Interactive Server, a component can use:

```razor
@using ArkheideSystem.Essential.Culture.Blazor
@inherits LocalizedComponentBase
@rendermode Microsoft.AspNetCore.Components.Web.RenderMode.InteractiveServer

<LocalizedText Key="Welcome_User" Arguments="@(new object?[] { "Blazor" })" />
<button @onclick="UseChinese">中文</button>

@code {
    private void UseChinese() => Localization.SetCulture("zh-CN");
}
```

`LocalizedComponentBase` subscribes to the scoped service and refreshes the component; `LocalizedText` renders encoded text. `ILocalizationService` can also be injected into other services. Each HTTP request or Interactive Server circuit keeps its own selection; immutable catalog data is shared. Do not use the process-wide desktop `Localizer` for user-specific Blazor language state.

The default scope initialization uses supported `CurrentUICulture` and `CurrentCulture`, falling back to the configured defaults; `InitializeWith` can supply host-owned state instead. Request localization middleware, language cookies, persistence and permitted format choices belong to the host. `SetCulture` changes the current scope only. For SSR/interactive initialization and cookie persistence, see the Blazor Gallery.

## Source generation

The Generator automatically discovers the project-root `Culture.json` and copies it to build and publish output. It generates public `CultureKey` and `Key` types in the consuming assembly. When a XAML adapter is referenced, it also generates the public `Localize` markup extension (an instance type, not a static entry point). The following is an abbreviated generated example:

```csharp
namespace ArkheideSystem.Essential.Culture;

public enum CultureKey
{
    Greeting,
}

public static class Key
{
    public static string Greeting => "Key.Greeting";
}
```

```csharp
using ArkheideSystem.Essential.Culture;
using GeneratedKey = global::ArkheideSystem.Essential.Culture.Key;

// Print the translated text
Console.WriteLine(Localizer.Parse(GeneratedKey.Greeting));

// Change the current culture
Localizer.Current.SetCulture("zh-CN");

// A new lookup uses the newly selected culture
Console.WriteLine(Localizer.Parse(GeneratedKey.Greeting));
```

> [!NOTE]
> The default culture and fallback culture are both `en-US`.

To override the namespace of generated types, add the following project setting:

```xml
<PropertyGroup>
  <EssentialCultureNamespace>ArkheideSystem.MyApplication.Localization</EssentialCultureNamespace>
</PropertyGroup>
```

Automatic creation never overwrites an existing `Culture.json`. To disable only template creation:

```xml
<PropertyGroup>
  <EssentialCultureAutoCreate>false</EssentialCultureAutoCreate>
</PropertyGroup>
```

`EssentialCultureAutoInclude=false` opts out of automatic resource preparation, creation, copying and module-output cleanup; the host then owns those tasks. `EssentialCultureGeneratorEnabled=false` disables C# generation and source diagnostics independently of copying and MSBuild module metadata validation. `EssentialCultureXamlFramework` defaults to `auto`; if multiple XAML adapters are referenced, explicitly choose `wpf`, `avalonia`, `winui` or `none`.

## Independent contexts

Use instance contexts for independent selections without changing ambient .NET cultures or the desktop facade:

```csharp
using ArkheideSystem.Essential.Culture;

var catalog = LocalizationCatalog.FromFile(
    Path.Combine(AppContext.BaseDirectory, "Culture.json"));
var english = new LocalizationContext(catalog, "en-US");
var chinese = new LocalizationContext(catalog, "zh-CN", formatCulture: "de-DE");
Console.WriteLine(chinese.Parse("Welcome_User", "Ada"));
chinese.SetCulture("en-US", "de-DE"); // english remains independent
```

`FromJson` and `Load(Stream)` also create catalogs; the caller retains ownership of the stream. Lookups accept raw keys or generated `Key.*` tokens. `Contains` checks declaration, `TryParse` returns `false` and an empty value for a missing key, and `Parse` returns a valid missing token unchanged. Invalid `Parse` tokens and formatting errors throw; `TryParse` does not suppress formatting errors. Typed one-, two- and three-argument overloads avoid allocating an argument array, while formatting still allocates the resulting string.

## Language policy and optional modules (1.4.0)

Existing single-file applications keep their default behavior. To restrict translation languages before creating contexts or desktop views:

```csharp
using ArkheideSystem.Essential.Culture;

var catalog = LocalizationCatalog.FromFile(
    Path.Combine(AppContext.BaseDirectory, "Culture.json"), "en-US",
    new CatalogLoadOptions(enabledCultures: ["en-US", "zh-CN"]));
Localizer.Configure(catalog, "en-US");
```

Call `Configure` once before any facade lookup, state access or event subscription, including before creating desktop views. Subsequent configuration is rejected. `Configure` accepts an optional independent formatting culture; later `Localizer.Current.SetCulture` follows the selected UI culture for formatting. Instance contexts and Blazor support separate formatting cultures on every change.

- With no policy (or empty default options), any valid culture request retains legacy exact → parent → configured fallback resolution; `AvailableCultures` lists authored languages rather than all valid requests.
- An explicit `enabledCultures` allowlist permits exactly those normalized requests, including valid custom tags. An empty allowlist permits none. Authored parents needed for allowed requests and the configured fallback are retained even if absent from that allowlist.
- A nonempty `disabledCultures` list with no allowlist permits declared languages except those exact tags. Thus undeclared requests are also rejected in this mode.
- Tags normalize casing and underscores. Disabling `fr` does not implicitly disable an allowed/declared `fr-CA`, but removes `fr` translations from its fallback chain. Allowing `fr-CA` may retain its authored `fr` parent unless that parent is explicitly disabled.
- Overlapping normalized allow/deny entries fail configuration; disabling the configured fallback fails loading. `DeclaredCultures` reports authored languages, `AvailableCultures` reports selectable policy requests, and `IsCultureEnabled` checks selection eligibility. Context construction and `SetCulture` enforce the same policy.

Formatting culture remains independent of the translation policy. All authored translations, including discarded ones, are strictly validated and temporarily allocated during startup; completed catalogs retain only the required translations. Filtering does not remove text from deployed JSON files. Populate language controls from `AvailableCultures` and update hard-coded commands to respect the policy.

For modules, declare files explicitly:

```xml
<ItemGroup>
  <CultureModule Include="Culture.MainPage.json" ModuleId="MainPage"
                 DeploymentPath="Pages/Culture.MainPage.json" />
</ItemGroup>
```

Explicit items work without enabling automatic discovery. The existing root `Culture.json` is included; when module items are present, a missing root template is not created. Alternatively:

```xml
<PropertyGroup>
  <EssentialCultureModulesEnabled>true</EssentialCultureModulesEnabled>
  <EssentialCultureFallbackCulture>en-US</EssentialCultureFallbackCulture>
</PropertyGroup>
```

This discovers only project-root `Culture.*.json`, excluding `Culture.options.json`; it does not recursively scan folders. Every file keeps `key -> culture -> string`; keys must be globally unique and each file must have consistent language sets and the configured fallback. Optional languages may differ between files. `EssentialCultureFallbackCulture` defaults to `en-US` and controls build-time validation and the manifest; changing it does not reconfigure the default static facade.

`ModuleId` defaults to the filename without its extension; `DeploymentPath` defaults to `Link`, or otherwise the filename. IDs and deployment paths must be unique without regard to case. IDs use ASCII letters/digits/`_`/`-`/`.`; deployment paths must be safe relative JSON paths without roots, traversal segments or reserved filenames. Declare explicit paths for linked resources with conflicting basenames.

The generator emits internal `CultureResources.Files` and `CultureResources.FallbackCulture` in the configured generated namespace:

```csharp
using ArkheideSystem.Essential.Culture; // Default generated namespace

var paths = CultureResources.Files.Select(path => Path.Combine(AppContext.BaseDirectory, path));
var catalog = LocalizationCatalog.FromFiles(paths, CultureResources.FallbackCulture,
    new CatalogLoadOptions(disabledCultures: ["zh-CN"]));
Localizer.Configure(catalog, culture: "en-US");
```

`CultureResources` is internal to the consuming assembly; import your configured generated namespace if you changed it. The manifest performs no initialization. Choose an enabled initial culture explicitly when using a different fallback or policy.

Module resources copy to their declared build/publish paths. Targets remove previously recorded stale module paths in the same output directory when declarations change, while protecting other current content. `FromFiles` eagerly reads and validates all supplied files, rejects duplicate canonical paths and reports duplicate keys with both source files. It does not defer loads, unload modules, or discover files at query time.

In Blazor, register the composed catalog through `AddCultureBlazor`/`AddCatalog`; host-supported UI cultures must be enabled by every registered catalog, or registration fails. Separate library catalogs retain independent key spaces and can use `ParseFrom`/`CatalogId`. Module validation reports source diagnostics `AEC001`–`AEC007`; invalid IDs/paths are also rejected by MSBuild even when generation is disabled.

Successful raw/token queries perform no file I/O and measured zero per-call allocation. Selection tables are shared by effective retained culture and bounded by retained language names. Startup validation and first selection still do work; no universal latency guarantee is implied. See the [performance measurements](https://github.com/Evigila/Essential/blob/master/docs-ai/current/culture-performance.md) for workloads and limits.

## Packages

| Package | Purpose |
| --- | --- |
| `Arkheide.Essential.Culture` | Culture resolution and the static `Localizer` entry point |
| `Arkheide.Essential.Culture.Generator` | Generates strongly typed keys from `Culture.json`; normally included transitively and does not need to be installed separately |
| `Arkheide.Essential.Culture.Wpf` | Strongly typed WPF `Localize` XAML binding |
| `Arkheide.Essential.Culture.Avalonia` | Strongly typed Avalonia `Localize` XAML binding |
| `Arkheide.Essential.Culture.WinUI` | Strongly typed WinUI 3 `Localize` support and window refresh infrastructure |
| `Arkheide.Essential.Culture.Blazor` | Request/circuit-scoped services and encoded text components |

```powershell
# Optional direct installation for a project that needs only source generation:
dotnet add package Arkheide.Essential.Culture.Generator
```

## AI assistance

> [!IMPORTANT]
> This library was developed with assistance from AI Agent (ChatGPT Codex).

## Documentation and examples

- [Single-file/XAML usage guide](https://github.com/Evigila/Essential/blob/master/src/Essential.Culture/docs/usage-guide.md) — existing guide; it does not cover the new 1.4.0 policy/module or Blazor configuration above.
- [1.4.0 resource configuration and manual checklist](https://github.com/Evigila/Essential/blob/master/docs-ai/current/culture-resource-usage.md)
- [Complete public API inventory](https://github.com/Evigila/Essential/blob/master/docs-ai/current/culture-public-api.md)
- [Console Gallery source](https://github.com/Evigila/Essential/tree/master/demo/Gallery.Essential.Culture)
- [WPF Gallery source](https://github.com/Evigila/Essential/tree/master/demo/Gallery.Essential.Culture.Wpf)
- [Avalonia Gallery source](https://github.com/Evigila/Essential/tree/master/demo/Gallery.Essential.Culture.Avalonia)
- [WinUI 3 Gallery source](https://github.com/Evigila/Essential/tree/master/demo/Gallery.Essential.Culture.WinUI)
- [Blazor Gallery source](https://github.com/Evigila/Essential/tree/master/demo/Gallery.Essential.Culture.Blazor)

## License

Licensed under the [MIT License](https://github.com/Evigila/Essential/blob/master/LICENSE.txt).
