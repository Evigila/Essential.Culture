# Project architecture and repository tree

**Status:** Verified maintained architecture inventory, updated for the authorized Culture 1.4.0 implementation and completed release. Existing naming/UI gaps are recorded below; inventory verification does not assert universal implementation compliance.

## Inventory scope

The inventory includes maintained files in the working tree rooted at `Essential/`, including intentionally untracked maintained files. The baseline is commit `49556163d2e72fe8ffa27cf5f942149823844bb0` plus the user's instruction/documentation migration, README changes and authorized Culture 1.4.0 implementation/release preparation.

| Excluded path or pattern | Reason |
| --- | --- |
| Root `docs/` and `docs-ai/` | Human and AI documentation subtrees excluded by the documentation contract |
| `.git/` | Version-control internals |
| `.vs/`, `.idea/` | Editor caches and workspace state |
| `artifacts/`, `TestResults/` | Generated packages, audit/verification outputs, temporary consumers and test results |
| Any `bin/` or `obj/` directory | Compiled output and generated intermediates |
| Any `node_modules/`, `.cache/`, `__pycache__/` directory | Dependency or execution caches |
| `*.user`, `*.suo`, `*.tmp`, `*.log` | User state and temporary output; no maintained file with these patterns was found |

The maintained nested `src/Essential.Culture/docs/usage-guide.md` is included: the documentation exclusion applies to the root area. Maintained source is not omitted solely because Git ignores it. Enumeration uses `rg --files --hidden --no-ignore` with the concrete exclusions above; all 125 maintained files and 17 project entries are verified against the filesystem.

## Complete repository tree

```text
Essential/
|-- .gitattributes
|-- .github/
|   \-- workflows/
|       \-- build.yml
|-- .gitignore
|-- AGENTS.ensure.json
|-- AGENTS.md
|-- demo/
|   |-- Culture.json
|   |-- Directory.Build.props
|   |-- Gallery.Essential.Culture/
|   |   |-- Gallery.Essential.Culture.csproj
|   |   |-- Program.cs
|   |   \-- README.md
|   |-- Gallery.Essential.Culture.Avalonia/
|   |   |-- App.axaml
|   |   |-- App.axaml.cs
|   |   |-- Gallery.Essential.Culture.Avalonia.csproj
|   |   |-- GreetingDialog.axaml
|   |   |-- GreetingDialog.axaml.cs
|   |   |-- MainWindow.axaml
|   |   |-- MainWindow.axaml.cs
|   |   |-- Program.cs
|   |   \-- README.md
|   |-- Gallery.Essential.Culture.Blazor/
|   |   |-- appsettings.json
|   |   |-- Components/
|   |   |   |-- _Imports.razor
|   |   |   |-- App.razor
|   |   |   |-- Layout/
|   |   |   |   \-- MainLayout.razor
|   |   |   |-- Pages/
|   |   |   |   \-- Home.razor
|   |   |   \-- Routes.razor
|   |   |-- Culture.json
|   |   |-- Gallery.Essential.Culture.Blazor.csproj
|   |   |-- Program.cs
|   |   |-- Properties/
|   |   |   \-- launchSettings.json
|   |   \-- wwwroot/
|   |       \-- app.css
|   |-- Gallery.Essential.Culture.slnx
|   |-- Gallery.Essential.Culture.WinUI/
|   |   |-- app.manifest
|   |   |-- App.xaml
|   |   |-- App.xaml.cs
|   |   |-- Gallery.Essential.Culture.WinUI.csproj
|   |   |-- MainWindow.xaml
|   |   |-- MainWindow.xaml.cs
|   |   |-- Properties/
|   |   |   \-- launchSettings.json
|   |   \-- README.md
|   |-- Gallery.Essential.Culture.Wpf/
|   |   |-- App.xaml
|   |   |-- App.xaml.cs
|   |   |-- Gallery.Essential.Culture.Wpf.csproj
|   |   |-- GreetingDialog.xaml
|   |   |-- GreetingDialog.xaml.cs
|   |   |-- MainWindow.xaml
|   |   |-- MainWindow.xaml.cs
|   |   \-- README.md
|   \-- README.md
|-- Directory.Build.props
|-- Essential.slnx
|-- LICENSE.txt
|-- publish-helper.bat
|-- scripts/
|   |-- Publish-Helper.ps1
|   |-- Release-Common.ps1
|   |-- ReleaseSettings.psd1
|   |-- Test-Release.ps1
|   |-- Verify-BlazorPackageConsumer.ps1
|   |-- Verify-ModulesPackageConsumer.ps1
|   \-- Verify-PackageSet.ps1
|-- src/
|   \-- Essential.Culture/
|       |-- Directory.Build.props
|       |-- docs/
|       |   \-- usage-guide.md
|       |-- Essential.Culture/
|       |   |-- CatalogLoadOptions.cs
|       |   |-- CultureName.cs
|       |   |-- Essential.Culture.csproj
|       |   |-- KeyToken.cs
|       |   |-- KeyValidation.cs
|       |   |-- LocalizationCatalog.cs
|       |   |-- LocalizationContext.cs
|       |   |-- LocalizationRuntime.cs
|       |   |-- Localizer.cs
|       |   \-- Properties/
|       |       \-- AssemblyInfo.cs
|       |-- Essential.Culture.Avalonia/
|       |   |-- AvaloniaLocalizeExtensionBase.cs
|       |   \-- Essential.Culture.Avalonia.csproj
|       |-- Essential.Culture.Blazor/
|       |   |-- Essential.Culture.Blazor.csproj
|       |   |-- ILocalizationService.cs
|       |   |-- LocalizationBuilder.cs
|       |   |-- LocalizationServiceCollectionExtensions.cs
|       |   |-- LocalizationSession.cs
|       |   |-- LocalizedComponentBase.cs
|       |   \-- LocalizedText.cs
|       |-- Essential.Culture.Generator/
|       |   |-- AnalyzerReleases.Shipped.md
|       |   |-- AnalyzerReleases.Unshipped.md
|       |   |-- buildTransitive/
|       |   |   |-- Arkheide.Essential.Culture.Generator.props
|       |   |   |-- Arkheide.Essential.Culture.Generator.targets
|       |   |   \-- Culture.template.json
|       |   |-- CultureDocument.cs
|       |   |-- CultureGenerator.cs
|       |   \-- Essential.Culture.Generator.csproj
|       |-- Essential.Culture.slnx
|       |-- Essential.Culture.WinUI/
|       |   |-- Essential.Culture.WinUI.csproj
|       |   |-- IWinUILocalizationHost.cs
|       |   |-- Properties/
|       |   |   \-- AssemblyInfo.cs
|       |   |-- WinUILocalizationHost.cs
|       |   |-- WinUILocalizationMarker.cs
|       |   \-- WinUILocalizeExtensionBase.cs
|       |-- Essential.Culture.Wpf/
|       |   |-- Essential.Culture.Wpf.csproj
|       |   \-- WpfLocalizeExtensionBase.cs
|       |-- README_zh-CN.md
|       \-- README.md
\-- tests/
    |-- Directory.Build.props
    |-- package-consumers/
    |   \-- Tests.Essential.Culture.Blazor.PackageConsumer/
    |       |-- Culture.json
    |       |-- Program.cs
    |       \-- Tests.Essential.Culture.Blazor.PackageConsumer.csproj
    |-- Tests.Essential.Culture/
    |   |-- AvaloniaLocalizeExtensionTests.cs
    |   |-- CatalogModuleTests.cs
    |   |-- CatalogPolicyTests.cs
    |   |-- Culture.json
    |   |-- LocalizationContextTests.cs
    |   |-- LocalizerConfigurationTests.cs
    |   |-- LocalizerTests.cs
    |   |-- TestAssembly.cs
    |   |-- Tests.Essential.Culture.csproj
    |   \-- WpfLocalizeExtensionTests.cs
    |-- Tests.Essential.Culture.Blazor/
    |   |-- LanguagePolicyTests.cs
    |   |-- LocalizationServiceTests.cs
    |   |-- LocalizedTextTests.cs
    |   \-- Tests.Essential.Culture.Blazor.csproj
    |-- Tests.Essential.Culture.Generator/
    |   |-- CultureGeneratorTests.cs
    |   |-- ModuleBuildTests.cs
    |   |-- ModuleGenerationTests.cs
    |   \-- Tests.Essential.Culture.Generator.csproj
    |-- Tests.Essential.Culture.Performance/
    |   |-- Program.cs
    |   \-- Tests.Essential.Culture.Performance.csproj
    \-- Tests.Essential.Culture.WinUI/
        |-- Tests.Essential.Culture.WinUI.csproj
        \-- WinUILocalizationMarkerTests.cs
```

## Root files and module responsibilities

| Path | Responsibility and evidence |
| --- | --- |
| `Essential.slnx` | Root development solution: six Culture libraries and four test projects, with links to focused module and Gallery solutions. |
| `Directory.Build.props` | Organization/package metadata, MIT license, repository URL, deterministic builds and `artifacts/packages` output; no module version. |
| `AGENTS.md`, `AGENTS.ensure.json` | Instruction router and audit state; state is not runtime configuration. |
| `.gitattributes`, `.gitignore`, `LICENSE.txt` | Text normalization, output/cache exclusions and repository license. |
| `.github/workflows/build.yml` | Windows .NET 10 restore/build/test/Gallery/pack validation and tag-triggered NuGet Trusted Publishing. |
| `publish-helper.bat`, `scripts/` | Release preparation/publishing entry points, exact package-set/repository/README validation, Trusted Publishing context preflight, isolated Blazor consumer and generated module consumer build/publish verification. |
| `src/Essential.Culture/` | Independently versioned localization module. Module props explicitly import root settings and set `VersionPrefix=1.4.0`; package repository/project URLs identify `https://github.com/Evigila/Essential`. |
| `tests/` | Four xUnit suites, an executable package-consumer fixture and optional standalone performance console; props import Culture settings. |
| `demo/` | Five runnable Gallery hosts; default NuGet mode and optional `UseLocalCulture=true` source mode; props import Culture settings. |

## Solution and project boundaries

This is a localization library family with adapters, tests and demonstrations; project boundaries do not establish a microservice system. In the table, library paths are relative to `src/Essential.Culture/`; other project paths are relative to the repository root. Each project has the same basename as its containing directory.

| Project directory | Kind/framework | Responsibility and reference direction |
| --- | --- | --- |
| `Essential.Culture/` | Shared library, `net10.0` | Complete JSON/placeholder validation, immutable catalogs, independent contexts and tokens; `CatalogLoadOptions` filters selectable/retained languages, `FromFiles` composes explicitly supplied modules with globally unique keys, and one-time `Localizer.Configure` initializes the desktop facade. Analyzer reference to Generator; no external runtime NuGet package. |
| `Essential.Culture.Generator/` | Roslyn incremental generator, `netstandard2.0` | Legacy single `Culture.json` or explicitly declared/discovered modules produce culture/key constants, enums and optional markup extensions. Per-file parsing/diagnostics separate content from key/manifest shape to preserve incremental key emission. Module `CultureResources` manifest records safe relative deployment paths and fallback; packages analyzer/buildTransitive props/targets/template. No Core project reference. |
| `Essential.Culture.Wpf/` | WPF adapter, `net10.0-windows` | One-way multi-bindings, argument/key bindings and synchronous culture notifications; references Core. The source raises PropertyChanged on the change-initiating thread without its own dispatcher forwarding. |
| `Essential.Culture.Avalonia/` | Avalonia adapter, `net10.0` | Framework bindings/conversion and culture refresh; references Core and Avalonia 12.1.0. |
| `Essential.Culture.WinUI/` | WinUI adapter, Windows 19041 target | Marker-producing markup extensions and disposable host that attaches windows, traverses supported display properties and refreshes using the dispatcher; references Core and Windows App SDK 2.3.1. |
| `Essential.Culture.Blazor/` | Razor library, `net10.0` | Named catalogs, request/circuit scoped contexts, DI registration, reactive components and independent format culture; validates supported/default cultures against every registered catalog's enabled policy. References Core and ASP.NET Core shared framework. |
| `tests/Tests.Essential.Culture/` | xUnit, `net10.0-windows`, WPF | Core loading/format/culture, module composition, enabled/disabled policies, atomic selection and isolated first-use facade configuration, plus WPF/Avalonia bindings; Core/Wpf/Avalonia/Generator analyzer references. |
| `tests/Tests.Essential.Culture.Generator/` | xUnit, `net10.0` | Legacy/module generation, per-file diagnostics, manifest/path validation and incremental invalidation tests; normal Generator reference plus Roslyn package. |
| `tests/Tests.Essential.Culture.Blazor/` | xUnit, `net10.0` | Catalog/session isolation, language-policy registration, encoding, formatting and reactive component tests; Blazor reference. |
| `tests/Tests.Essential.Culture.WinUI/` | xUnit, Windows 19041 target | Four marker tests; WinUI reference. Host/window acceptance requires manual verification. |
| `tests/package-consumers/Tests.Essential.Culture.Blazor.PackageConsumer/` | Executable Razor fixture, `net10.0` | Installs only Blazor (default 1.4.0), checks transitive generated keys/Core/scoped localization/rendering. Outside root solution; release script invokes it. |
| `tests/Tests.Essential.Culture.Performance/` | Optional console, `net10.0` | Stopwatch/allocation/GC/working-set probe for catalog loading, selection and lookup across key/language/module sizes. References current Core or optional `BaselineCatalogProject`; outside solutions and standard release test loop; no external benchmark package. |
| `demo/Gallery.Essential.Culture/` | Console executable, `net10.0` | Generated tokens and global loading/parsing/culture switching with shared `demo/Culture.json`. |
| `demo/Gallery.Essential.Culture.Wpf/` | WPF executable, `net10.0-windows` | Main window/dialog, live culture refresh and arguments. |
| `demo/Gallery.Essential.Culture.Avalonia/` | Avalonia desktop executable, `net10.0` | Main window/dialog with Avalonia.Desktop and Fluent theme. |
| `demo/Gallery.Essential.Culture.WinUI/` | Unpackaged WinUI executable, `win-x64`, Windows 19041 target | Windows App SDK self-contained mode; app attaches its window to localization host. |
| `demo/Gallery.Essential.Culture.Blazor/` | ASP.NET Core executable, `net10.0` | SSR/Interactive Server; embedded catalog, antiforgery-validated culture POST and local redirect/cookie persistence. |

The focused source solution contains the same six libraries and four tests as the root solution; the Gallery solution contains five executable hosts. The optional performance console and package-consumer fixture remain outside the solutions. Release verification also generates a temporary module-only Core consumer under ignored `artifacts/`, builds/runs it from fresh candidate packages and runs published output to check deployment paths. Adapter packages depend on Core, which carries Generator as an analyzer dependency. [1_dependency.md](1_dependency.md) enumerates package and project edges.

## Runtime and resource boundaries

| Application/resource | Process/resource boundary | Communication or persistence | Evidence |
| --- | --- | --- | --- |
| Core and desktop facade | Loaded in consumer process | Explicit file/string/stream ingestion and module composition, startup language policy, immutable snapshots and one-time facade configuration; no runtime directory scanning/network translation service | `CatalogLoadOptions.cs`, `LocalizationCatalog.cs`, `LocalizationContext.cs`, `Localizer.cs`, `LocalizationRuntime.cs` |
| Desktop Galleries | Four processes: console/WPF/Avalonia/WinUI | Shared `demo/Culture.json` copied to output; global desktop culture, framework dispatching | Projects and entry points |
| Blazor Gallery | ASP.NET Core host, contexts scoped per request/circuit | Embedded `Demo.Texts.json`; HTTP culture form, cookie, Accept-Language initialization and Interactive Server events | `Program.cs`, project resources, `Home.razor` |
| Generator | C# compilation/build deployment | AdditionalFiles/compiler-visible properties, explicit `CultureModule` metadata and opt-in bounded root discovery; targets validate module identities/paths, copy resources for build/publish and clean only previously tracked stale JSON outputs within the same output root; generated manifest records relative paths | Generator source, `CultureDocument.cs`, props/targets |
| NuGet.org/GitHub Actions | Build/distribution | NuGet v3 restore/push, release tags, `Evigila/Essential`, `build.yml`, `nuget` environment and OIDC; local preflight checks context, remote policy authentication occurs in NuGet/login | Workflow/release settings/scripts |
| Flourish integration | External repository | No Essential project references Flourish; optional bridges/controls belong outside this checkout | ProjectReference inventory, UIUX rules and integration guide |

Catalogs share lazily initialized, immutable raw/token lookup dictionaries by effective retained culture. Arbitrary requested child tags reuse their deepest retained parent or fallback, so the cache is bounded by retained culture names. Contexts retain independent culture/format state and atomically replace that state; resource I/O and JSON validation remain startup operations. These implementation boundaries support predictable hot lookup without claiming an optimal result for every workload.

No database, broker, container deployment or production-host topology is declared in the inspected repository. Consumer-specific resources remain outside this library's verified boundary.

## Active constraints and compliance gaps

- Solution/project basenames follow product/role-prefix standards. Existing test/Gallery `AssemblyName` values retain earlier identities; renaming a project does not authorize assembly identity changes.
- Most owned namespaces use `ArkheideSystem.`. `LocalizationServiceCollectionExtensions.cs` uses `Microsoft.Extensions.DependencyInjection` for conventional discovery. The consumer fixture sets `RootNamespace=BlazorOnly`, generated namespace `PackageConsumer.Texts` and has global top-level `Program`. These existing declarations conflict with the current prefix requirement. A compatibility-aware migration or scoped authorized exception remains necessary.
- Generator namespace is configurable; generator tests exercise the consumer namespace `Demo.Generated`. The fixture choice does not prove universal compliance with owned-namespace rules. The Blazor Gallery also declares a public partial `Program` in the global namespace alongside its top-level entry point.
- Current UIUX rules require applicable Flourish controls/styles. Existing Gallery XAML uses framework-native controls; Blazor `Home.razor` uses native `button`/`select` and `wwwroot/app.css` defines host control styles. No Flourish dependency or inspected Flourish implementation is present in this checkout. Resolving this existing UI gap requires actual contracts and a scoped implementation decision.
- Each JSON module retains a uniform culture set across its own keys; modules may declare different sets while sharing the configured fallback and globally unique keys. Policies use normalized exact tags; disabled tags are excluded from selectable names and retained parent-fallback translations. A context enforces policy before atomically replacing its state. These are implemented 1.4.0 startup features; arbitrary runtime module/plugin discovery and hot reload are outside this implementation.
- Existing human README/usage/demo material is retained. Stale versions/links must be reported within their ownership boundary; this inventory does not authorize rewriting human material.

## Verification and unresolved facts

Release follow-up: source commit `53ecbeb94c4d5e14f2776fbc5048b34d1471cc50` and annotated `v1.4.0` were pushed. The renamed-repository workflow passed Trusted Publishing and six uploads; all six 1.4.0 packages became publicly downloadable and passed metadata/assets verification. Fresh public-only Blazor and module consumers built and ran with zero warnings/errors, including module publish/execution. See [completed release verification](release-verification.md#completed-140-publication-on-2026-10-09). The following paragraph retains the local preparation boundary before those remote actions.

Inspected the complete 125-file inventory, three solutions, 17 maintained project declarations, current props/release files and affected runtime/adapter/host source. The independent `artifacts/culture-1.4-final.log` records 201 passing tests (Core 89, Generator 82, Blazor 26, WinUI marker 4), root and all five source-mode Galleries built with zero warnings/errors, and fresh Blazor-only consumer success. After the final Generator output-ledger adjustment, the Generator candidate was repacked and the complete six-package set was verified for metadata/README/license/dependency/API-documentation/assets, including SHA-256 agreement between packaged buildTransitive files and current source. The latest fresh module consumer built/ran with zero warnings/errors and ran again from published output (`artifacts/culture-1.4-final-modules.log`). All five package-mode Galleries also restored/built with zero warnings/errors using a fresh cache and explicit candidate-source mapping (`artifacts/culture-1.4-gallery-packages.log`). No Git tag or publication was performed. Performance probing is optional and separate from xUnit/real UI acceptance. The exact 23 current dependency graphs and historical package evidence are recorded in [1_dependency.md](1_dependency.md).

The initial audit's 21 restore graphs and 106 tests (61/20/21/4) remain dated 1.3.0 historical evidence, as do preliminary 1.4.0 runs. They do not replace the final independent preparation above.

The inventory is complete for this checkout. The compliance/external-source gaps above remain unresolved. Historical release/test results in existing records are not reruns performed by this documentation audit.
