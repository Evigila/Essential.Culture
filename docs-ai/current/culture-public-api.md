# Current Culture public API inventory

**Status:** Verified source inventory, not a proposed API or a release guarantee.

**Inspection:** 2026-10-09, America/Sao_Paulo (UTC-03:00). Inspected the working-tree implementation based on Git revision `49556163d2e72fe8ffa27cf5f942149823844bb0`, including the approved resource implementation. The module declares candidate version `1.4.0` in [its build properties](../../src/Essential.Culture/Directory.Build.props). This version has not been published by this task.

This inventory covers every project-owned public type and its declared public/protected members in the six production projects, consumer-generated types, and the generator's MSBuild and diagnostic contracts. It contains **19 handwritten public types**, counting the nested `Localizer.Current` type. Consumer-generated types are additional and depend on that consumer's input. Ordinary inherited `System.Object` members and the complete APIs of third-party base classes are not repeated; their inheritance is identified. Internal types, public members inside inaccessible nested types, test helpers, and Gallery application types are not external library APIs.

The associated [optimization plan](culture-optimization-plan.md) distinguishes implemented stages 1/2 from optional future work. [Resource configuration](culture-resource-usage.md) explains policy and modular startup.

## Packages, namespaces, and type count

| Production project / package | Current target framework | Public types | Namespace |
| --- | --- | --- | --- |
| `Essential.Culture` / `Arkheide.Essential.Culture` | `net10.0` | `LocalizationCatalog`, `LocalizationContext`, `Localizer`, `Localizer.Current`, `KeyToken`, `CultureName`, `CatalogLoadOptions` (7) | `ArkheideSystem.Essential.Culture` |
| `Essential.Culture.Blazor` / `Arkheide.Essential.Culture.Blazor` | `net10.0`, framework reference to `Microsoft.AspNetCore.App` | `ILocalizationService`, `LocalizationSelection`, `LocalizationBuilder`, `LocalizedComponentBase`, `LocalizedText`, `LocalizationServiceCollectionExtensions` (6) | `ArkheideSystem.Essential.Culture.Blazor`, except the DI extension type in `Microsoft.Extensions.DependencyInjection` |
| `Essential.Culture.Wpf` / `Arkheide.Essential.Culture.Wpf` | `net10.0-windows` | `WpfLocalizeExtensionBase` (1) | `ArkheideSystem.Essential.Culture.Wpf` |
| `Essential.Culture.Avalonia` / `Arkheide.Essential.Culture.Avalonia` | `net10.0` | `AvaloniaLocalizeExtensionBase` (1) | `ArkheideSystem.Essential.Culture.Avalonia` |
| `Essential.Culture.WinUI` / `Arkheide.Essential.Culture.WinUI` | `net10.0-windows10.0.19041.0`; minimum platform `10.0.17763.0` | `WinUILocalizeExtensionBase`, `IWinUILocalizationHost`, `WinUILocalizationHost` (3) | `ArkheideSystem.Essential.Culture.WinUI` |
| `Essential.Culture.Generator` / `Arkheide.Essential.Culture.Generator` | `netstandard2.0` | `CultureGenerator` (1) | `ArkheideSystem.Essential.Culture.Generator` |

Evidence: [core project](../../src/Essential.Culture/Essential.Culture/Essential.Culture.csproj), [Blazor project](../../src/Essential.Culture/Essential.Culture.Blazor/Essential.Culture.Blazor.csproj), [WPF project](../../src/Essential.Culture/Essential.Culture.Wpf/Essential.Culture.Wpf.csproj), [Avalonia project](../../src/Essential.Culture/Essential.Culture.Avalonia/Essential.Culture.Avalonia.csproj), [WinUI project](../../src/Essential.Culture/Essential.Culture.WinUI/Essential.Culture.WinUI.csproj), and [generator project](../../src/Essential.Culture/Essential.Culture.Generator/Essential.Culture.Generator.csproj).

## Core library

All seven core types belong to `ArkheideSystem.Essential.Culture`. Existing type numbering is retained; the additive options type is listed as 19 below.

### 1. LocalizationCatalog

`public sealed class LocalizationCatalog` owns validated, immutable translations. Its constructor is private; consumers use factories. [source](../../src/Essential.Culture/Essential.Culture/LocalizationCatalog.cs).

| Public member | Meaning | Source |
| --- | --- | --- |
| `IReadOnlyList<string> AvailableCultures { get; }` | Selectable requests under an explicit policy; otherwise all declared names. | [source](../../src/Essential.Culture/Essential.Culture/LocalizationCatalog.cs) |
| `IReadOnlyList<string> DeclaredCultures { get; }` | Normalized authored union across all files, including excluded languages. | [source](../../src/Essential.Culture/Essential.Culture/LocalizationCatalog.cs) |
| `string FallbackCulture { get; }` | The configured fallback translation culture. | [source](../../src/Essential.Culture/Essential.Culture/LocalizationCatalog.cs) |
| `static LocalizationCatalog FromJson(string json, string fallbackCulture = "en-US")` | Parse and validate a JSON string; currently converts it to UTF-8 before loading. | [source](../../src/Essential.Culture/Essential.Culture/LocalizationCatalog.cs) |
| `static LocalizationCatalog Load(Stream stream, string fallbackCulture = "en-US")` | Read from the stream's current position. The caller retains ownership; validation failures do not close the stream. | [source](../../src/Essential.Culture/Essential.Culture/LocalizationCatalog.cs) |
| `static LocalizationCatalog FromFile(string path, string fallbackCulture = "en-US")` | Read a catalog from an explicitly supplied file path. | [source](../../src/Essential.Culture/Essential.Culture/LocalizationCatalog.cs) |
| `static LocalizationCatalog FromJson(string json, string fallbackCulture, CatalogLoadOptions options)` | Fully validate JSON and filter retained languages. | [source](../../src/Essential.Culture/Essential.Culture/LocalizationCatalog.cs) |
| `static LocalizationCatalog Load(Stream stream, string fallbackCulture, CatalogLoadOptions options)` | Apply policy while preserving stream ownership/current-position behavior. | [source](../../src/Essential.Culture/Essential.Culture/LocalizationCatalog.cs) |
| `static LocalizationCatalog FromFile(string path, string fallbackCulture, CatalogLoadOptions options)` | Apply policy to an explicit file. | [source](../../src/Essential.Culture/Essential.Culture/LocalizationCatalog.cs) |
| `static LocalizationCatalog FromFiles(IEnumerable<string> paths, string fallbackCulture = "en-US", CatalogLoadOptions? options = null)` | Eagerly compose files with unique global keys; validate each file's own language set. | [source](../../src/Essential.Culture/Essential.Culture/LocalizationCatalog.cs) |
| `bool IsCultureEnabled(string culture)` | Normalize and test the exact selectable request. Invalid tags throw; legacy catalogs accept all valid requests. | [source](../../src/Essential.Culture/Essential.Culture/LocalizationCatalog.cs) |

The schema is one root object mapping raw keys to culture-to-string objects. Keys must be representable C# identifiers. Every key must include the configured fallback, declare the same culture set within its own file, contain nonempty string translations, and preserve the fallback's sorted composite-format placeholder index sequence, including repeated occurrences. Optional languages may differ between modules. JSON comments, trailing commas, duplicate global keys, and duplicate normalized culture names are rejected. Excluded translations still undergo full validation. Evidence: [catalog validation](../../src/Essential.Culture/Essential.Culture/LocalizationCatalog.cs).

Catalogs can be shared between independent contexts. Composite formats are parsed during loading. Frozen selection tables are shared by effective retained culture, with per-selection coordinated publication; arbitrary requests cannot grow the cache beyond retained names. Factories return immutable catalogs. Runtime mutation/reload/unload and deferred loading are not exposed. [Selection implementation](../../src/Essential.Culture/Essential.Culture/LocalizationCatalog.cs).

### 2. LocalizationContext

`public sealed class LocalizationContext` resolves a shared catalog with independent translation and formatting selections. [source](../../src/Essential.Culture/Essential.Culture/LocalizationContext.cs).

| Public member | Meaning | Source |
| --- | --- | --- |
| `LocalizationContext(LocalizationCatalog catalog, string culture = "en-US", string? formatCulture = null)` | Create an independent context without file I/O. Null formatting culture follows the selected translation culture. | [source](../../src/Essential.Culture/Essential.Culture/LocalizationContext.cs) |
| `string Culture { get; }` | Current normalized translation culture name. | [source](../../src/Essential.Culture/Essential.Culture/LocalizationContext.cs) |
| `IReadOnlyList<string> AvailableCultures { get; }` | The catalog's selectable requests. | [source](../../src/Essential.Culture/Essential.Culture/LocalizationContext.cs) |
| `IReadOnlyList<string> DeclaredCultures { get; }` | The catalog's authored union. | [source](../../src/Essential.Culture/Essential.Culture/LocalizationContext.cs) |
| `CultureInfo FormatCulture { get; }` | Effective read-only formatting culture, independent of ambient thread/process culture. | [source](../../src/Essential.Culture/Essential.Culture/LocalizationContext.cs) |
| `event EventHandler? Changed` | Synchronous notification after a changed selection pair is committed. | [source](../../src/Essential.Culture/Essential.Culture/LocalizationContext.cs) |
| `void SetCulture(string culture, string? formatCulture = null)` | Atomically replace both selections; an equivalent normalized pair is a no-op. | [source](../../src/Essential.Culture/Essential.Culture/LocalizationContext.cs) |
| `bool Contains(string token)` | Test whether a raw key or generated token is declared. | [source](../../src/Essential.Culture/Essential.Culture/LocalizationContext.cs) |

The ten public lookup overloads are listed in the shared lookup table below. Without policy, the context accepts valid requests outside `AvailableCultures`. With policy, construction and switching enforce the exact request list, while enabled requests resolve permitted parent translations and fallback. Its `Culture` property retains the requested normalized name even when translations fall back. Disabled translation tags never participate in resolution. [Selection](../../src/Essential.Culture/Essential.Culture/LocalizationCatalog.cs).

Each lookup reads one immutable selection snapshot. `SetCulture` commits under a lock and raises `Changed` outside it. Event handler exceptions propagate after the state has changed; concurrent changes do not guarantee callback ordering, and a handler can observe a subsequent selection. The APIs do not mutate `CurrentCulture`, `CurrentUICulture`, or process-wide culture defaults. Unusable custom formatting cultures fall back to `InvariantCulture`. [source](../../src/Essential.Culture/Essential.Culture/LocalizationContext.cs) and [source](../../src/Essential.Culture/Essential.Culture/LocalizationContext.cs).

### 3. Localizer

`public static class Localizer` is the process-wide desktop facade. It has no accessible constructor. It exposes the ten lookup overloads below as static methods, plus `static bool Contains(string token)` at [source](../../src/Essential.Culture/Essential.Culture/Localizer.cs). [source](../../src/Essential.Culture/Essential.Culture/Localizer.cs).

The additive `static void Configure(LocalizationCatalog catalog, string culture = "en-US", string? formatCulture = null)` initializes the facade once before any state access, lookup or subscription. Configuration performs no file loading; pass a previously loaded single/modular catalog. Later configuration is rejected, including after failed default initialization. If unconfigured, the facade initializes lazily from `Path.Combine(AppContext.BaseDirectory, "Culture.json")`, with initial/fallback `en-US`; default initialization failures retain the existing sticky fault behavior. The initial formatting culture can be supplied through configuration; `Current.SetCulture` retains its existing single-argument signature. This global state is unsuitable for independent server-side Blazor users. [Initialization](../../src/Essential.Culture/Essential.Culture/LocalizationRuntime.cs).

### 4. Localizer.Current

`public static class Current` is nested inside `Localizer` and has no accessible constructor. [source](../../src/Essential.Culture/Essential.Culture/Localizer.cs).

| Public static member | Meaning | Source |
| --- | --- | --- |
| `string Culture { get; }` | Process-wide selected culture name. | [source](../../src/Essential.Culture/Essential.Culture/Localizer.cs) |
| `IReadOnlyList<string> AvailableCultures { get; }` | Selectable requests in the configured catalog, or declared cultures of the legacy default file. | [source](../../src/Essential.Culture/Essential.Culture/Localizer.cs) |
| `event EventHandler? Changed` | Subscribe to or unsubscribe from global selection changes. | [source](../../src/Essential.Culture/Essential.Culture/Localizer.cs) |
| `void SetCulture(string culture)` | Change the culture used by future global queries. | [source](../../src/Essential.Culture/Essential.Culture/Localizer.cs) |

Subscriptions are strong event subscriptions and consumers must release their own subscriptions when appropriate. This type does not expose the core context's independent `formatCulture` parameter.

### Complete lookup overloads on LocalizationContext and Localizer

Each signature below is an instance member of `LocalizationContext` and a `static` member of `Localizer`.

| Exact signature (excluding `static`) | Purpose | Context source | Localizer source |
| --- | --- | --- | --- |
| `string Parse(string token)` | Resolve without formatting arguments. | [source](../../src/Essential.Culture/Essential.Culture/LocalizationContext.cs) | [source](../../src/Essential.Culture/Essential.Culture/Localizer.cs) |
| `string Parse(string token, params object?[] arguments)` | Resolve and optionally format with an array. | [source](../../src/Essential.Culture/Essential.Culture/LocalizationContext.cs) | [source](../../src/Essential.Culture/Essential.Culture/Localizer.cs) |
| `string Parse<TArg0>(string token, TArg0 argument0)` | Format with one typed argument. | [source](../../src/Essential.Culture/Essential.Culture/LocalizationContext.cs) | [source](../../src/Essential.Culture/Essential.Culture/Localizer.cs) |
| `string Parse<TArg0, TArg1>(string token, TArg0 argument0, TArg1 argument1)` | Format with two typed arguments. | [source](../../src/Essential.Culture/Essential.Culture/LocalizationContext.cs) | [source](../../src/Essential.Culture/Essential.Culture/Localizer.cs) |
| `string Parse<TArg0, TArg1, TArg2>(string token, TArg0 argument0, TArg1 argument1, TArg2 argument2)` | Format with three typed arguments. | [source](../../src/Essential.Culture/Essential.Culture/LocalizationContext.cs) | [source](../../src/Essential.Culture/Essential.Culture/Localizer.cs) |
| `bool TryParse(string token, out string value)` | Attempt lookup without arguments. | [source](../../src/Essential.Culture/Essential.Culture/LocalizationContext.cs) | [source](../../src/Essential.Culture/Essential.Culture/Localizer.cs) |
| `bool TryParse(string token, object?[] arguments, out string value)` | Attempt lookup and format with an array. | [source](../../src/Essential.Culture/Essential.Culture/LocalizationContext.cs) | [source](../../src/Essential.Culture/Essential.Culture/Localizer.cs) |
| `bool TryParse<TArg0>(string token, TArg0 argument0, out string value)` | Attempt lookup with one typed argument. | [source](../../src/Essential.Culture/Essential.Culture/LocalizationContext.cs) | [source](../../src/Essential.Culture/Essential.Culture/Localizer.cs) |
| `bool TryParse<TArg0, TArg1>(string token, TArg0 argument0, TArg1 argument1, out string value)` | Attempt lookup with two typed arguments. | [source](../../src/Essential.Culture/Essential.Culture/LocalizationContext.cs) | [source](../../src/Essential.Culture/Essential.Culture/Localizer.cs) |
| `bool TryParse<TArg0, TArg1, TArg2>(string token, TArg0 argument0, TArg1 argument1, TArg2 argument2, out string value)` | Attempt lookup with three typed arguments. | [source](../../src/Essential.Culture/Essential.Culture/LocalizationContext.cs) | [source](../../src/Essential.Culture/Essential.Culture/Localizer.cs) |

Both raw keys and stable `Key.*` tokens are accepted, with case-sensitive key matching. `Parse` returns the input for a valid but unknown key/token and throws `ArgumentException` for an invalid unknown value. `TryParse` returns `false` and an empty output when lookup fails. It does not suppress composite-format errors, and array overloads reject null arrays. Calling without arguments returns the template text, including any placeholders. Generic overloads use the typed `CompositeFormat` formatting path without creating an argument array; this is not a claim that every format operation is allocation-free. [source](../../src/Essential.Culture/Essential.Culture/LocalizationContext.cs).

### 5. KeyToken

`public static class KeyToken` has no accessible constructor. [source](../../src/Essential.Culture/Essential.Culture/KeyToken.cs).

| Public static member | Meaning | Source |
| --- | --- | --- |
| `const string Prefix = "Key."` | Stable generated-token prefix. | [source](../../src/Essential.Culture/Essential.Culture/KeyToken.cs) |
| `string Create(string key)` | Validate a raw key and prepend the prefix. | [source](../../src/Essential.Culture/Essential.Culture/KeyToken.cs) |
| `bool TryGetKey(string token, out string key)` | Validate and extract a raw key from either a raw key or prefixed token. It does not check a catalog. | [source](../../src/Essential.Culture/Essential.Culture/KeyToken.cs) |

Allowed keys contain only ASCII letters, digits, and underscores; the first character cannot be a digit. C# keywords and generated/reserved names `Key`, `CultureKey`, and `value__` are excluded. [source](../../src/Essential.Culture/Essential.Culture/KeyValidation.cs).

### 6. CultureName

`public static class CultureName` has no accessible constructor. Its sole member is `public static string Normalize(string culture)` at [source](../../src/Essential.Culture/Essential.Culture/CultureName.cs).

Normalization trims whitespace, replaces `_` with `-`, and canonicalizes casing while retaining syntactically valid custom culture names. Empty/invalid names throw `ArgumentException`. A normalized name does not establish catalog membership, host permission, or usable installed culture data. [source](../../src/Essential.Culture/Essential.Culture/KeyValidation.cs).

### 19. CatalogLoadOptions

`public sealed class CatalogLoadOptions` defines immutable startup policy in the core namespace. [Source](../../src/Essential.Culture/Essential.Culture/CatalogLoadOptions.cs).

| Public member | Meaning |
| --- | --- |
| `CatalogLoadOptions(IEnumerable<string>? enabledCultures = null, IEnumerable<string>? disabledCultures = null)` | Copy and normalize exact tag lists. Reject overlapping normalized allow/deny names. |
| `IReadOnlyList<string>? EnabledCultures { get; }` | Explicit selectable requests; null means declared names except denied; an empty list enables no requests. |
| `IReadOnlyList<string> DisabledCultures { get; }` | Exact names excluded from selection and retained translations, including parent resolution. |

Fallback cannot be disabled; it stays retained even if not selectable. An enabled custom request may retain permitted authored parents. Formatting culture remains independent. Passing empty options preserves legacy behavior. Options do not remove text from deployment files and do not support mutation after loading.

## Blazor integration

Types 7–11 use `ArkheideSystem.Essential.Culture.Blazor`. Type 12 uses the actual DI extension namespace listed below.

### 7. ILocalizationService

`public interface ILocalizationService` is the scoped translation contract. [source](../../src/Essential.Culture/Essential.Culture.Blazor/ILocalizationService.cs).

| Member | Meaning | Source |
| --- | --- | --- |
| `string Culture { get; }` | This scope's selected translation culture. | [source](../../src/Essential.Culture/Essential.Culture.Blazor/ILocalizationService.cs) |
| `CultureInfo FormatCulture { get; }` | This scope's effective read-only formatting culture. | [source](../../src/Essential.Culture/Essential.Culture.Blazor/ILocalizationService.cs) |
| `IReadOnlyList<string> AvailableCultures { get; }` | UI cultures allowed by host configuration. | [source](../../src/Essential.Culture/Essential.Culture.Blazor/ILocalizationService.cs) |
| `event EventHandler? Changed` | Synchronous notification after this scope's selection changes. | [source](../../src/Essential.Culture/Essential.Culture.Blazor/ILocalizationService.cs) |
| `void SetCulture(string culture, string? formatCulture = null)` | Change this scope; null formatting culture follows UI culture. | [source](../../src/Essential.Culture/Essential.Culture.Blazor/ILocalizationService.cs) |
| `string Parse(string token, params object?[] arguments)` | Resolve in the default catalog. | [source](../../src/Essential.Culture/Essential.Culture.Blazor/ILocalizationService.cs) |
| `string ParseFrom(string catalogId, string token, params object?[] arguments)` | Resolve in a named catalog. | [source](../../src/Essential.Culture/Essential.Culture.Blazor/ILocalizationService.cs) |
| `bool TryParse(string token, out string value)` | Attempt default-catalog lookup. | [source](../../src/Essential.Culture/Essential.Culture.Blazor/ILocalizationService.cs) |
| `bool TryParse(string token, object?[] arguments, out string value)` | Attempt default-catalog lookup and formatting. | [source](../../src/Essential.Culture/Essential.Culture.Blazor/ILocalizationService.cs) |
| `bool TryParseFrom(string catalogId, string token, out string value)` | Attempt named-catalog lookup. | [source](../../src/Essential.Culture/Essential.Culture.Blazor/ILocalizationService.cs) |
| `bool TryParseFrom(string catalogId, string token, object?[] arguments, out string value)` | Attempt named-catalog lookup and formatting. | [source](../../src/Essential.Culture/Essential.Culture.Blazor/ILocalizationService.cs) |
| `bool Contains(string token)` | Check the default catalog. | [source](../../src/Essential.Culture/Essential.Culture.Blazor/ILocalizationService.cs) |
| `bool ContainsFrom(string catalogId, string token)` | Check a named catalog. | [source](../../src/Essential.Culture/Essential.Culture.Blazor/ILocalizationService.cs) |

There are no typed generic formatting overloads on this interface. Unknown catalog identities throw `KeyNotFoundException`, including through `TryParseFrom`; `Try` only concerns key lookup. The implementation rejects a UI culture outside the host's supported list. Formatting culture is separate. Multi-catalog state changes are prepared as a new snapshot before publication; the implementation does not change ambient cultures. [source](../../src/Essential.Culture/Essential.Culture.Blazor/LocalizationSession.cs) and [source](../../src/Essential.Culture/Essential.Culture.Blazor/LocalizationSession.cs).

### 8. LocalizationSelection

The declaration is `public sealed record LocalizationSelection(string Culture, string? FormatCulture = null)` at [source](../../src/Essential.Culture/Essential.Culture.Blazor/ILocalizationService.cs). It describes initial scope selection and performs no independent validation or state change.

The positional record exposes the following compiler-generated public surface:

- `LocalizationSelection(string Culture, string? FormatCulture = null)`.
- `string Culture { get; init; }` and `string? FormatCulture { get; init; }`.
- `void Deconstruct(out string Culture, out string? FormatCulture)`.
- `bool Equals(LocalizationSelection? other)` via `IEquatable<LocalizationSelection>`.
- Overrides of `bool Equals(object? obj)`, `int GetHashCode()`, and `string ToString()`.
- `static bool operator ==(LocalizationSelection? left, LocalizationSelection? right)` and the corresponding `!=` operator.
- Record cloning used by `with`; the compiler emits a public method with a reserved, non-C#-callable name for this operation. Consumers should use record syntax instead of depending on that metadata name.

The synthesized copy constructor, equality-contract property, and print helper of this sealed record are private and do not add public/protected extension points. Its only intended construction overload is the positional constructor above.

### 9. LocalizationBuilder

`public sealed class LocalizationBuilder` has an **internal** constructor and is obtained through the DI registration callback. [source](../../src/Essential.Culture/Essential.Culture.Blazor/LocalizationBuilder.cs) and [source](../../src/Essential.Culture/Essential.Culture.Blazor/LocalizationBuilder.cs).

| Public member | Meaning | Source |
| --- | --- | --- |
| `LocalizationBuilder AddCatalog(string catalogId, LocalizationCatalog catalog)` | Register an immutable catalog with a unique, case-sensitive identity; the first becomes the default unless replaced. | [source](../../src/Essential.Culture/Essential.Culture.Blazor/LocalizationBuilder.cs) |
| `LocalizationBuilder SetDefaultCatalog(string catalogId)` | Select the catalog for queries without an explicit identity. | [source](../../src/Essential.Culture/Essential.Culture.Blazor/LocalizationBuilder.cs) |
| `LocalizationBuilder SetDefaultCulture(string culture, string? formatCulture = null)` | Configure the fallback initial selection. | [source](../../src/Essential.Culture/Essential.Culture.Blazor/LocalizationBuilder.cs) |
| `LocalizationBuilder AddSupportedCultures(params string[] cultures)` | Add normalized allowed UI cultures, ignoring duplicates. | [source](../../src/Essential.Culture/Essential.Culture.Blazor/LocalizationBuilder.cs) |
| `LocalizationBuilder InitializeWith(Func<IServiceProvider, LocalizationSelection> initialize)` | Resolve each scope's initial selection from host-owned services/state. | [source](../../src/Essential.Culture/Essential.Culture.Blazor/LocalizationBuilder.cs) |

All five methods return this builder. Completion requires a registered default catalog and a default culture in the supported UI list. Without an explicitly populated supported list, the default catalog's cultures are used. Reusing a captured builder after completion throws. The allowlist can deliberately include a culture not declared by a catalog; that catalog then uses its normal fallback. These APIs do not remove translations or constrain core/desktop contexts. [source](../../src/Essential.Culture/Essential.Culture.Blazor/LocalizationBuilder.cs).

Without an initializer, new scopes reference ambient `CurrentUICulture` and `CurrentCulture`, using configured defaults where necessary. The library has no built-in cookie, browser storage, HTTP, or preference persistence contract. [source](../../src/Essential.Culture/Essential.Culture.Blazor/LocalizationSession.cs).

### 10. LocalizedComponentBase

`public abstract class LocalizedComponentBase : ComponentBase, IDisposable` at [source](../../src/Essential.Culture/Essential.Culture.Blazor/LocalizedComponentBase.cs) has an implicit protected parameterless constructor.

| Declared public/protected member | Meaning | Source |
| --- | --- | --- |
| `[Inject] protected ILocalizationService Localization { get; set; }` | Scoped service available to subclasses. | [source](../../src/Essential.Culture/Essential.Culture.Blazor/LocalizedComponentBase.cs) |
| `protected override void OnInitialized()` | Subscribe to the service's change event. Derived initialization overrides must call base. | [source](../../src/Essential.Culture/Essential.Culture.Blazor/LocalizedComponentBase.cs) |
| `public virtual void Dispose()` | Idempotently unsubscribe from the initialized service. Derived disposal overrides must preserve this cleanup. | [source](../../src/Essential.Culture/Essential.Culture.Blazor/LocalizedComponentBase.cs) |

Third-party inherited surface comes from `Microsoft.AspNetCore.Components.ComponentBase`, including its component interfaces, public `SetParametersAsync`, and protected rendering/lifecycle extension points. Language notifications dispatch rendering through the renderer; disposal prevents queued refreshes from rendering after disposal. [source](../../src/Essential.Culture/Essential.Culture.Blazor/LocalizedComponentBase.cs).

### 11. LocalizedText

`public sealed class LocalizedText : LocalizedComponentBase` at [source](../../src/Essential.Culture/Essential.Culture.Blazor/LocalizedText.cs) has an implicit public parameterless constructor and inherits `Dispose` and the framework component contract.

| Declared public/protected member | Meaning | Source |
| --- | --- | --- |
| `[Parameter, EditorRequired] public string Key { get; set; }` | Raw key or stable token; default is empty, and rendering validates it. | [source](../../src/Essential.Culture/Essential.Culture.Blazor/LocalizedText.cs) |
| `[Parameter] public string? CatalogId { get; set; }` | Null uses the default catalog. | [source](../../src/Essential.Culture/Essential.Culture.Blazor/LocalizedText.cs) |
| `[Parameter] public object?[] Arguments { get; set; }` | Composite-format arguments; default is an empty array. | [source](../../src/Essential.Culture/Essential.Culture.Blazor/LocalizedText.cs) |
| `[Parameter] public string? Fallback { get; set; }` | Literal text for a missing key; null preserves normal `Parse` behavior. | [source](../../src/Essential.Culture/Essential.Culture.Blazor/LocalizedText.cs) |
| `protected override void BuildRenderTree(RenderTreeBuilder builder)` | Render encoded text and validate required inputs. | [source](../../src/Essential.Culture/Essential.Culture.Blazor/LocalizedText.cs) |

The component does not render translation strings as raw HTML and has no `ChildContent` or own formatting-culture parameter. Formatting selection belongs to its scoped service. `EditorRequired` is not a substitute for runtime input validation.

### 12. LocalizationServiceCollectionExtensions

The actual namespace is **`Microsoft.Extensions.DependencyInjection`**. The static type has no accessible constructor. [Namespace/type declaration, lines 3 and 6](../../src/Essential.Culture/Essential.Culture.Blazor/LocalizationServiceCollectionExtensions.cs#L3).

Its sole method is:

```csharp
public static IServiceCollection AddCultureBlazor(
    this IServiceCollection services,
    Action<LocalizationBuilder> configure);
```

[source](../../src/Essential.Culture/Essential.Culture.Blazor/LocalizationServiceCollectionExtensions.cs). It registers immutable singleton options and scoped `ILocalizationService`, returning the supplied collection. Duplicate localization-service registration throws. This inventory records the existing namespace; any naming-policy migration is a separate compatibility decision.

## Desktop XAML adapters

### 13. WpfLocalizeExtensionBase

`public abstract class WpfLocalizeExtensionBase : System.Windows.Markup.MarkupExtension`, namespace `ArkheideSystem.Essential.Culture.Wpf`. Type attributes are `[ContentProperty(nameof(Arguments))]` and `[MarkupExtensionReturnType(typeof(object))]`. [source](../../src/Essential.Culture/Essential.Culture.Wpf/WpfLocalizeExtensionBase.cs).

| Declared public/protected member | Meaning | Source |
| --- | --- | --- |
| `protected WpfLocalizeExtensionBase()` | Create an uninitialized extension for a derived facade. | [source](../../src/Essential.Culture/Essential.Culture.Wpf/WpfLocalizeExtensionBase.cs) |
| `protected WpfLocalizeExtensionBase(string token)` | Create an extension with a selected token. | [source](../../src/Essential.Culture/Essential.Culture.Wpf/WpfLocalizeExtensionBase.cs) |
| `protected string Token { get; set; }` | Derived facade's token; reading before initialization throws. | [source](../../src/Essential.Culture/Essential.Culture.Wpf/WpfLocalizeExtensionBase.cs) |
| `public BindingBase? KeyBinding { get; set; }` | Binding that supplies the runtime key/token. | [source](../../src/Essential.Culture/Essential.Culture.Wpf/WpfLocalizeExtensionBase.cs) |
| `public BindingBase? Arg0 { get; set; }` | First argument binding. | [source](../../src/Essential.Culture/Essential.Culture.Wpf/WpfLocalizeExtensionBase.cs) |
| `public BindingBase? Arg1 { get; set; }` | Second argument binding. | [source](../../src/Essential.Culture/Essential.Culture.Wpf/WpfLocalizeExtensionBase.cs) |
| `public BindingBase? Arg2 { get; set; }` | Third argument binding. | [source](../../src/Essential.Culture/Essential.Culture.Wpf/WpfLocalizeExtensionBase.cs) |
| `public Collection<BindingBase> Arguments { get; }` | Ordered content collection for any number of argument bindings. | [source](../../src/Essential.Culture/Essential.Culture.Wpf/WpfLocalizeExtensionBase.cs) |
| `public override object ProvideValue(IServiceProvider serviceProvider)` | Create/evaluate the OneWay MultiBinding used by XAML. | [source](../../src/Essential.Culture/Essential.Culture.Wpf/WpfLocalizeExtensionBase.cs) |

The static token and `KeyBinding` are mutually exclusive. One of them must be supplied. `Arguments` cannot be combined with `Arg0`–`Arg2`; `Arg1` requires `Arg0`, and `Arg2` requires `Arg1`. A binding refreshes when key, arguments, or global language change. The internal signal raises `PropertyChanged` on the change-calling thread and does not dispatch it itself; hosts must respect WPF thread ownership. [source](../../src/Essential.Culture/Essential.Culture.Wpf/WpfLocalizeExtensionBase.cs).

Third-party inheritance is the WPF `MarkupExtension` contract. This adapter uses the process-wide `Localizer` and does not expose a catalog identity or independent window context.

### 14. AvaloniaLocalizeExtensionBase

`public abstract class AvaloniaLocalizeExtensionBase`, namespace `ArkheideSystem.Essential.Culture.Avalonia`, has no third-party class base. [source](../../src/Essential.Culture/Essential.Culture.Avalonia/AvaloniaLocalizeExtensionBase.cs).

| Declared public/protected member | Meaning | Source |
| --- | --- | --- |
| `protected AvaloniaLocalizeExtensionBase()` | Create an uninitialized derived extension. | [source](../../src/Essential.Culture/Essential.Culture.Avalonia/AvaloniaLocalizeExtensionBase.cs) |
| `protected AvaloniaLocalizeExtensionBase(string token)` | Create an extension with a token. | [source](../../src/Essential.Culture/Essential.Culture.Avalonia/AvaloniaLocalizeExtensionBase.cs) |
| `protected string? Token { get; set; }` | Token supplied by the derived facade. | [source](../../src/Essential.Culture/Essential.Culture.Avalonia/AvaloniaLocalizeExtensionBase.cs) |
| `public BindingBase? KeyBinding { get; set; }` | Runtime key/token binding. | [source](../../src/Essential.Culture/Essential.Culture.Avalonia/AvaloniaLocalizeExtensionBase.cs) |
| `public BindingBase? Arg0 { get; set; }` | First argument binding. | [source](../../src/Essential.Culture/Essential.Culture.Avalonia/AvaloniaLocalizeExtensionBase.cs) |
| `public BindingBase? Arg1 { get; set; }` | Second argument binding. | [source](../../src/Essential.Culture/Essential.Culture.Avalonia/AvaloniaLocalizeExtensionBase.cs) |
| `public BindingBase? Arg2 { get; set; }` | Third argument binding. | [source](../../src/Essential.Culture/Essential.Culture.Avalonia/AvaloniaLocalizeExtensionBase.cs) |
| `[Content] public IList<BindingBase> Arguments { get; }` | Ordered argument binding content. | [source](../../src/Essential.Culture/Essential.Culture.Avalonia/AvaloniaLocalizeExtensionBase.cs) |
| `public MultiBinding ProvideValue(IServiceProvider serviceProvider)` | Return a OneWay binding for key, argument, and culture changes. | [source](../../src/Essential.Culture/Essential.Culture.Avalonia/AvaloniaLocalizeExtensionBase.cs) |

Key and argument mutual-exclusion/ordering rules match WPF. Notifications originating off the UI thread are posted through `Dispatcher.UIThread`. [source](../../src/Essential.Culture/Essential.Culture.Avalonia/AvaloniaLocalizeExtensionBase.cs). The adapter also uses process-wide `Localizer`; there is no catalog identity or independent context member.

### 15. WinUILocalizeExtensionBase

`public abstract class WinUILocalizeExtensionBase : Microsoft.UI.Xaml.Markup.MarkupExtension`, namespace `ArkheideSystem.Essential.Culture.WinUI`. [source](../../src/Essential.Culture/Essential.Culture.WinUI/WinUILocalizeExtensionBase.cs).

| Declared public/protected member | Meaning | Source |
| --- | --- | --- |
| `protected WinUILocalizeExtensionBase()` | Create an extension without a static token, permitting a dynamic marker. | [source](../../src/Essential.Culture/Essential.Culture.WinUI/WinUILocalizeExtensionBase.cs) |
| `protected WinUILocalizeExtensionBase(string token)` | Create an extension with a static token. | [source](../../src/Essential.Culture/Essential.Culture.WinUI/WinUILocalizeExtensionBase.cs) |
| `protected string Token { get; set; }` | Derived facade's token; reading before selection throws. | [source](../../src/Essential.Culture/Essential.Culture.WinUI/WinUILocalizeExtensionBase.cs) |
| `protected override object ProvideValue()` | Return a marker for subsequent host resolution. | [source](../../src/Essential.Culture/Essential.Culture.WinUI/WinUILocalizeExtensionBase.cs) |

The complete public attached-property surface is:

| Attached input | Public field and exact accessors | Source |
| --- | --- | --- |
| Dynamic key | `static readonly DependencyProperty KeyBindingProperty`; `static string? GetKeyBinding(DependencyObject target)`; `static void SetKeyBinding(DependencyObject target, string? value)` | [source](../../src/Essential.Culture/Essential.Culture.WinUI/WinUILocalizeExtensionBase.cs), [source](../../src/Essential.Culture/Essential.Culture.WinUI/WinUILocalizeExtensionBase.cs), [source](../../src/Essential.Culture/Essential.Culture.WinUI/WinUILocalizeExtensionBase.cs) |
| Argument 0 | `static readonly DependencyProperty Argument0Property`; `static object? GetArgument0(DependencyObject target)`; `static void SetArgument0(DependencyObject target, object? value)` | [source](../../src/Essential.Culture/Essential.Culture.WinUI/WinUILocalizeExtensionBase.cs), [source](../../src/Essential.Culture/Essential.Culture.WinUI/WinUILocalizeExtensionBase.cs), [source](../../src/Essential.Culture/Essential.Culture.WinUI/WinUILocalizeExtensionBase.cs) |
| Argument 1 | `static readonly DependencyProperty Argument1Property`; `static object? GetArgument1(DependencyObject target)`; `static void SetArgument1(DependencyObject target, object? value)` | [source](../../src/Essential.Culture/Essential.Culture.WinUI/WinUILocalizeExtensionBase.cs), [source](../../src/Essential.Culture/Essential.Culture.WinUI/WinUILocalizeExtensionBase.cs), [source](../../src/Essential.Culture/Essential.Culture.WinUI/WinUILocalizeExtensionBase.cs) |
| Argument 2 | `static readonly DependencyProperty Argument2Property`; `static object? GetArgument2(DependencyObject target)`; `static void SetArgument2(DependencyObject target, object? value)` | [source](../../src/Essential.Culture/Essential.Culture.WinUI/WinUILocalizeExtensionBase.cs), [source](../../src/Essential.Culture/Essential.Culture.WinUI/WinUILocalizeExtensionBase.cs), [source](../../src/Essential.Culture/Essential.Culture.WinUI/WinUILocalizeExtensionBase.cs) |
| Arbitrary arguments | `static readonly DependencyProperty ArgumentsProperty`; `static IList<object?>? GetArguments(DependencyObject target)`; `static void SetArguments(DependencyObject target, IList<object?>? value)` | [source](../../src/Essential.Culture/Essential.Culture.WinUI/WinUILocalizeExtensionBase.cs), [source](../../src/Essential.Culture/Essential.Culture.WinUI/WinUILocalizeExtensionBase.cs), [source](../../src/Essential.Culture/Essential.Culture.WinUI/WinUILocalizeExtensionBase.cs) |

Unlike WPF/Avalonia instance binding properties, these inputs are attached to the target object and use names `Argument0`–`Argument2`. A non-null `Arguments` list takes precedence over indexed inputs. Mutating elements inside a previously assigned list does not itself trigger the attached dependency-property callback; replace the value or reapply localization when required. [source](../../src/Essential.Culture/Essential.Culture.WinUI/WinUILocalizeExtensionBase.cs).

The inherited third-party surface is WinUI's `MarkupExtension`/WinRT projection contract, including the additional protected service-provider `ProvideValue` overload. The extension itself is not a `DependencyObject`; the attached input APIs operate on separate target objects. Marker resolution and automatic language updates require the host described below.

### 16. IWinUILocalizationHost

`public interface IWinUILocalizationHost : IDisposable`, namespace `ArkheideSystem.Essential.Culture.WinUI`. [source](../../src/Essential.Culture/Essential.Culture.WinUI/IWinUILocalizationHost.cs).

| Member | Meaning | Source |
| --- | --- | --- |
| `void Attach(Window window)` | Start automatic tracking; repeated attachment is ignored. | [source](../../src/Essential.Culture/Essential.Culture.WinUI/IWinUILocalizationHost.cs) |
| `void Detach(Window window)` | Stop tracking the window. | [source](../../src/Essential.Culture/Essential.Culture.WinUI/IWinUILocalizationHost.cs) |
| `void Apply(DependencyObject root)` | Discover/localize an independent visual root, such as dialog/popup content. | [source](../../src/Essential.Culture/Essential.Culture.WinUI/IWinUILocalizationHost.cs) |
| `void Dispose()` | Inherited from `System.IDisposable`; release automatic tracking. | [source](../../src/Essential.Culture/Essential.Culture.WinUI/IWinUILocalizationHost.cs) |

### 17. WinUILocalizationHost

`public sealed class WinUILocalizationHost : IWinUILocalizationHost`, namespace `ArkheideSystem.Essential.Culture.WinUI`. [source](../../src/Essential.Culture/Essential.Culture.WinUI/WinUILocalizationHost.cs).

| Public member | Meaning | Source |
| --- | --- | --- |
| `WinUILocalizationHost()` | Create the lifecycle host. | [source](../../src/Essential.Culture/Essential.Culture.WinUI/WinUILocalizationHost.cs) |
| `void Attach(Window window)` | Track window events, discover initial values, and subscribe to global culture changes. | [source](../../src/Essential.Culture/Essential.Culture.WinUI/WinUILocalizationHost.cs) |
| `void Detach(Window window)` | Release an attached window and its tracking ownership. | [source](../../src/Essential.Culture/Essential.Culture.WinUI/WinUILocalizationHost.cs) |
| `void Apply(DependencyObject root)` | Discover marked values under a root and associate tracking with a matching registered window where possible. | [source](../../src/Essential.Culture/Essential.Culture.WinUI/WinUILocalizationHost.cs) |
| `void Dispose()` | Idempotently cancel global subscriptions and release all window registrations, dispatching cleanup where necessary. | [source](../../src/Essential.Culture/Essential.Culture.WinUI/WinUILocalizationHost.cs) |

`Attach`, `Detach`, and `Apply` require the owning UI thread. Culture changes enqueue refreshes on each registered window's dispatcher and coalesce already queued refreshes. Closed windows detach automatically. Calling `Apply` without a matching attached window can resolve values immediately but does not establish subsequent automatic culture refresh for that root. Newly inserted or independent content can need another `Apply`; culture refresh updates tracked values rather than rescanning the complete tree. [source](../../src/Essential.Culture/Essential.Culture.WinUI/WinUILocalizationHost.cs) and [source](../../src/Essential.Culture/Essential.Culture.WinUI/WinUILocalizationHost.cs).

Supported display properties are `Window.Title`, `TextBlock.Text`, `ContentControl.Content`, `ContentDialog.Title`, `ContentDialog.PrimaryButtonText`, `ContentDialog.SecondaryButtonText`, `ContentDialog.CloseButtonText`, the `PlaceholderText` properties on `TextBox`, `PasswordBox`, `RichEditBox`, and `AutoSuggestBox`, `ToolTipService.ToolTip`, and `AutomationProperties.Name`. There is no public rule registration for arbitrary dependency properties. A dynamic `KeyBinding` cannot localize `Window.Title`. The host uses process-wide `Localizer` and has no independent culture/catalog selection. [source](../../src/Essential.Culture/Essential.Culture.WinUI/WinUILocalizationHost.cs), [source](../../src/Essential.Culture/Essential.Culture.WinUI/WinUILocalizationHost.cs), and [source](../../src/Essential.Culture/Essential.Culture.WinUI/WinUILocalizationHost.cs).

## Source generator and generated consumer contracts

### 18. CultureGenerator

`[Generator(LanguageNames.CSharp)] public sealed class CultureGenerator : IIncrementalGenerator`, namespace `ArkheideSystem.Essential.Culture.Generator`. [source](../../src/Essential.Culture/Essential.Culture.Generator/CultureGenerator.cs).

- Implicit public parameterless constructor `CultureGenerator()`.
- `public void Initialize(IncrementalGeneratorInitializationContext context)` at [source](../../src/Essential.Culture/Essential.Culture.Generator/CultureGenerator.cs).

The interface and incremental-generation context belong to Roslyn. The package places the generator assembly under `analyzers/dotnet/cs`; it is an analyzer entry point, not a normal runtime service library. [Package configuration](../../src/Essential.Culture/Essential.Culture.Generator/Essential.Culture.Generator.csproj).

### Generated CultureKey and Key

For a valid consumer input, the configured namespace receives:

```csharp
public enum CultureKey
{
    // One member per valid JSON key, ordered with StringComparer.Ordinal.
}

public static class Key
{
    // For each raw key <Name>:
    public static string <Name> => "Key.<Name>";
}
```

`<Name>` is schematic notation, not literal C# source. [source](../../src/Essential.Culture/Essential.Culture.Generator/CultureGenerator.cs). `CultureKey` inherits the normal `System.Enum` contract; `Key` has no accessible constructor. Generated types carry `GeneratedCodeAttribute` and are owned by the consumer's assembly, not fixed exported types from the core library assembly.

The key properties return stable tokens, not localized text. Enum values are implicit consecutive integers after sorting; inserting earlier keys changes later numeric values, so the generator does not establish persistent integer identities. The runtime has no `Parse(CultureKey)` overload. The complete key/member list is the selected consumer's document, not a universal package key list.

### Generated Localize

When a XAML framework is selected, the consumer also receives:

```csharp
public sealed class Localize : <SelectedAdapterBase>
{
    public Localize();
    public CultureKey Key { get; set; }
}
```

The schematic base is `WpfLocalizeExtensionBase`, `AvaloniaLocalizeExtensionBase`, or `WinUILocalizeExtensionBase`. [source](../../src/Essential.Culture/Essential.Culture.Generator/CultureGenerator.cs). Setting `Key` selects a token; an invalid enum value throws `ArgumentOutOfRangeException`. The parameterless constructor does not itself initialize a static token. WPF/Avalonia therefore require either setting `Key` or providing `KeyBinding`; WinUI permits the parameterless extension to produce a dynamic marker.

The generated class inherits its adapter's public members and framework contract. In the WinUI case it additionally declares `public new static` forwarders for **all five dependency-property fields and all ten attached Get/Set methods** listed in the WinUI base table above. Field types, getter/setter parameter types, nullability, and behavior are identical to the base; fields refer to the same dependency-property instances. This includes `KeyBindingProperty`, `Argument0Property`, `Argument1Property`, `Argument2Property`, `ArgumentsProperty`, `GetKeyBinding`/`SetKeyBinding`, `GetArgument0`/`SetArgument0`, `GetArgument1`/`SetArgument1`, `GetArgument2`/`SetArgument2`, and `GetArguments`/`SetArguments`. [source](../../src/Essential.Culture/Essential.Culture.Generator/CultureGenerator.cs).

The typical static XAML contract is `{culture:Localize Key=<EnumMember>}`. WPF/Avalonia dynamic keys use the extension's `KeyBinding` property; WinUI places `Localize.KeyBinding` on the target object and uses `{culture:Localize}` as the display value. Actual hosts demonstrate [WPF use](../../demo/Gallery.Essential.Culture.Wpf/MainWindow.xaml), [Avalonia use](../../demo/Gallery.Essential.Culture.Avalonia/MainWindow.axaml), and [WinUI use](../../demo/Gallery.Essential.Culture.WinUI/MainWindow.xaml).

### MSBuild configuration, including legacy names

The five established properties have legacy fallback aliases; the two new module properties do not. A legacy value is used only when the corresponding current property is empty. [source](../../src/Essential.Culture/Essential.Culture.Generator/buildTransitive/Arkheide.Essential.Culture.Generator.props).

| Current property | Default | Accepted behavior/value | Exact legacy fallback alias |
| --- | --- | --- | --- |
| `EssentialCultureNamespace` | `ArkheideSystem.Essential.Culture` | A syntactically valid C# namespace for generated types; consumers may configure another namespace. | `ArkheideEssentialCultureNamespace` |
| `EssentialCultureGeneratorEnabled` | `auto` | `auto`, `true`, or `false`; empty behaves as auto. Auto emits nothing without resources; true requires resources. Multiple resources require module integration. False emits nothing. | `ArkheideEssentialCultureGeneratorEnabled` |
| `EssentialCultureAutoInclude` | `true` | A literal MSBuild value `false` leaves resource inclusion, copying and cleanup to the host; otherwise the targets prepare declared resources and the root document. | `ArkheideEssentialCultureAutoInclude` |
| `EssentialCultureAutoCreate` | `true` | A literal value `false` prevents copying the template to a missing root document. | `ArkheideEssentialCultureAutoCreate` |
| `EssentialCultureXamlFramework` | `auto` | `auto`, `wpf`, `avalonia`, `winui`, or `none`; the implementation also accepts `false` as none. Empty behaves as auto. | `ArkheideEssentialCultureXamlFramework` |
| `EssentialCultureModulesEnabled` | `false` | Opt in to root-directory `Culture.*.json` discovery, excluding `Culture.options.json`. Explicit `CultureModule` items work independently. | None |
| `EssentialCultureFallbackCulture` | `en-US` | The normalized fallback required during build validation and emitted in the internal manifest. Runtime callers must use the matching fallback. | None |

Generator mode/framework values are trimmed and interpreted without case sensitivity. Auto framework detection selects no facade when no adapter is referenced and selects the single available adapter otherwise. Multiple available adapters require an explicit selection; selecting a missing adapter is an error. [source](../../src/Essential.Culture/Essential.Culture.Generator/CultureGenerator.cs) and [source](../../src/Essential.Culture/Essential.Culture.Generator/CultureGenerator.cs).

`AdditionalFiles` remains supported for the traditional `Culture.json`. `CultureModule` items add optional files with `ModuleId` and `DeploymentPath` metadata. The targets pass `CultureModule`, `CultureModuleId` and `CultureDeploymentPath` through compiler-visible AdditionalFiles metadata. Explicit item declarations enable composition; the discovery property controls automatic root globs. IDs and safe deployment-relative paths must be unique, and global raw keys cannot overlap. [Generator](../../src/Essential.Culture/Essential.Culture.Generator/CultureGenerator.cs) and [targets](../../src/Essential.Culture/Essential.Culture.Generator/buildTransitive/Arkheide.Essential.Culture.Generator.targets).

The preparation targets retain root template/inclusion behavior and detect module declarations before automatic creation. Declared resources copy to their deployment-relative output/publish paths with `PreserveNewest`; target-owned obsolete copied resources are removed when declarations change. Generator disabling and automatic inclusion remain separate switches. Module integration also emits internal `CultureResources.Files` (read-only relative paths) and `CultureResources.FallbackCulture`; these are assembly-internal host startup helpers, not exported package API. They perform no I/O or static-facade initialization. [Targets](../../src/Essential.Culture/Essential.Culture.Generator/buildTransitive/Arkheide.Essential.Culture.Generator.targets).

### Diagnostic contracts

All seven descriptors are enabled errors in category `Arkheide.Essential.Culture`.

| ID | Existing trigger/meaning | Source |
| --- | --- | --- |
| `AEC001` | Required resources are missing, or multiple traditional documents have no module integration. | [source](../../src/Essential.Culture/Essential.Culture.Generator/CultureGenerator.cs) |
| `AEC002` | Invalid JSON, duplicate root/normalized culture names, empty text, culture/fallback/format mismatch, no valid keys, or invalid namespace. | [source](../../src/Essential.Culture/Essential.Culture.Generator/CultureGenerator.cs) |
| `AEC003` | A key cannot be represented by the generated identifier contracts. | [source](../../src/Essential.Culture/Essential.Culture.Generator/CultureGenerator.cs) |
| `AEC004` | Invalid `EssentialCultureGeneratorEnabled` mode. | [source](../../src/Essential.Culture/Essential.Culture.Generator/CultureGenerator.cs) |
| `AEC005` | Invalid/ambiguous XAML framework configuration or a selected adapter is not referenced. | [source](../../src/Essential.Culture/Essential.Culture.Generator/CultureGenerator.cs) |
| `AEC006` | Invalid/duplicate module identity, unsafe/duplicate deployment path, or invalid module configuration. | [source](../../src/Essential.Culture/Essential.Culture.Generator/CultureGenerator.cs) |
| `AEC007` | A global key is declared in multiple files; diagnostics include both source locations. | [source](../../src/Essential.Culture/Essential.Culture.Generator/CultureGenerator.cs) |

The generator parses and validates individual files before collection, separating translation diagnostics from a value-equatable key/identity shape. Translation-only changes revalidate the affected resource and reuse unchanged generated shape. It validates authored text, normalized language sets, configured fallback and format placeholder occurrences. Runtime loading still validates deployment files, since they may differ from the build inputs. [Parser](../../src/Essential.Culture/Essential.Culture.Generator/CultureDocument.cs) and [generator](../../src/Essential.Culture/Essential.Culture.Generator/CultureGenerator.cs).

## Current capability boundaries and verification

The core has immutable shared catalogs, independent contexts, raw/stable-token lookup, explicit formatting cultures, typed 1–3 argument overloads, startup language policy, eager modular composition and one-time desktop configuration. Blazor retains isolated named catalogs and scoped selection, and rejects host supported lists that violate any registered catalog's policy. Runtime unloading, deferred module readiness and alternate streaming/index storage remain optional future work.

This inventory is checked against source/configuration. [Release verification](release-verification.md) records executed checks separately. No Computer Use acceptance is claimed. Package/assembly IDs and existing interface signatures are preserved.
