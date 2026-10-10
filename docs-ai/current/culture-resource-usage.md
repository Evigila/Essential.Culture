# Culture 1.4.0 resource configuration

This describes the implemented eager loading contract. Every JSON file keeps the existing `key -> culture -> string` format. Existing single-file applications need no configuration change.

## Startup language policy

```csharp
var options = new CatalogLoadOptions(
    enabledCultures: ["en-US", "zh-CN"],
    disabledCultures: ["fr"]);
var catalog = LocalizationCatalog.FromFile("Culture.json", "en-US", options);
var context = new LocalizationContext(catalog, "en-US", "de-DE");
```

`CatalogLoadOptions` copies, normalizes, sorts and deduplicates its inputs. An omitted allowlist selects declared languages except explicitly disabled names. An explicit allowlist selects exactly those requests, including valid custom tags. Conflicting normalized allow/deny entries fail configuration. Disabling the fallback fails loading. An empty allowlist yields no selectable cultures; constructing a context then fails until a usable configuration is supplied.

`DeclaredCultures` lists the authored union. `AvailableCultures` lists selectable requests under the policy. `IsCultureEnabled` checks a request using the same policy as context construction and `SetCulture`. Without a configured policy, arbitrary valid requests retain legacy parent/fallback resolution.

The host owns language controls and persistence. Populate a policy-aware picker from `AvailableCultures`, and update hard-coded language commands to respect `IsCultureEnabled`. The original Galleries demonstrate the default single-file configuration.

Disablement applies to exact normalized tags. Disabling `fr` does not implicitly disable a declared `fr-CA`; however, `fr` translations are not retained and cannot supply its parent fallback. An allowlist containing `fr-CA` can retain an authored `fr` parent unless it is explicitly disabled. The fallback remains retained even if it is absent from the allowlist. Formatting culture is independent and is not restricted by the translation policy.

All translations are still strictly validated, including discarded languages. Temporary JSON/text/composite-format allocations occur during validation; excluded translation objects do not remain in the completed catalog. This feature restricts retained memory, not the contents of deployed JSON files or access to their text.

## Functional modules

Keep common keys in `Culture.json`, and put feature keys in files such as `Culture.MainPage.json` and `Culture.Changelog.json`. Keys are globally unique within the resulting logical catalog. Each file must define its own consistent culture set and contain the configured fallback for every key; optional languages may differ between files.

Explicit project items work without enabling automatic discovery:

```xml
<PropertyGroup>
  <EssentialCultureNamespace>ArkheideSystem.MyApp.Texts</EssentialCultureNamespace>
  <EssentialCultureFallbackCulture>en-US</EssentialCultureFallbackCulture>
</PropertyGroup>
<ItemGroup>
  <CultureModule Include="Culture.MainPage.json"
                 ModuleId="MainPage" DeploymentPath="Pages/Culture.MainPage.json" />
  <CultureModule Include="../Shared/Culture.Changelog.json"
                 ModuleId="Changelog" DeploymentPath="Shared/Culture.Changelog.json" />
</ItemGroup>
```

Alternatively, set `EssentialCultureModulesEnabled=true` to discover root-directory `Culture.*.json` files, excluding `Culture.options.json`. This discovery does not recursively scan folders. The existing root file remains included. Explicit module declarations suppress automatic creation of an otherwise missing root template. `EssentialCultureAutoInclude=false` leaves resource inclusion/copying to the host; generator disablement remains a separate property.

`ModuleId` defaults to the filename without its extension. `DeploymentPath` defaults to `Link` or the filename. IDs and deployment paths must be unique. Deployment paths must be safe relative paths; rooted paths and traversal segments are rejected. Set explicit identities/paths for linked resources with conflicting basenames.

When modules are present, the generator adds an internal `CultureResources` class to the configured namespace. Its read-only `Files` list and `FallbackCulture` identify the resources. Translation-only edits do not change generated key/manifest text. Generated `Key`, `CultureKey` and platform `Localize` shapes remain compatible. The generated manifest performs no automatic initialization or file I/O.

```csharp
using ArkheideSystem.MyApp.Texts;

var paths = CultureResources.Files
    .Select(path => Path.Combine(AppContext.BaseDirectory, path));
var catalog = LocalizationCatalog.FromFiles(paths, CultureResources.FallbackCulture,
    new CatalogLoadOptions(disabledCultures: ["fr"]));
Localizer.Configure(catalog, "en-US");
Console.WriteLine(Localizer.Parse(Key.Welcome));
```

Call `Localizer.Configure` once, before any facade state access, lookup or subscription. This matters for desktop bindings whose constructors may subscribe early. If it is never called, the facade retains its lazy `Culture.json` default. Configuration after first use is rejected. Applications using instance contexts do not need the static facade.

`FromFiles` eagerly reads the supplied paths and validates all modules before returning. It rejects duplicate canonical file paths and reports duplicate keys with both source files. Lookup performs no directory discovery or I/O. Module files are copied to their declared deployment paths during build and publish; target-owned stale paths are removed when declarations disappear.

## Blazor

Compose related module files into one catalog, then register that catalog with `AddCatalog`. Keep independent libraries as separately named catalogs. Host supported cultures must be enabled by every registered catalog; inconsistent configuration fails during registration. Each request/circuit keeps its own selected culture, while immutable translations and bounded selection tables are shared. Existing `ILocalizationService` implementations require no new interface members.

## Manual acceptance

1. Start each existing Gallery with its original single file; compare all texts and formatting before and after switching languages.
2. In a policy-configured test host, populate language controls from `AvailableCultures`. Confirm a disabled language is absent, and direct normalized selection fails without changing the displayed text or raising a change event. In Blazor, also confirm a conflicting host supported list fails at startup.
3. Split keys among common and feature modules. Check common/feature text, typed formatting, dynamic bindings and dialogs. Inspect build and publish paths, then delete a module declaration and confirm its previously copied output is removed.
4. Configure the desktop facade before creating views. Check repeated configuration rejection, multiple windows, background switches, and adapter disposal on the owning UI thread.
5. Open two independent Blazor sessions. Change one language, check the other remains unchanged, and verify host persistence, SSR/interactive initial language and encoded output.
6. Confirm NuGet policy fields match `Evigila/Essential`, `build.yml`, environment `nuget`, the intended account and all six existing package IDs. After an authorized tag release, separately verify successful login/uploads, public indexing and fresh-cache public-only consumption.

Deferred module loading, runtime unload/disable, streaming parsing and alternate lookup storage are not implemented. The approved plan reserves them for workloads with measured need. No Computer Use acceptance is claimed.
