# Project dependency inventory

**Status:** Current declarations and the complete 23-graph local preparation snapshot were rechecked for AGENTS framework integration. The 67 external package/version pairs are unchanged; all six Culture packages resolved locally in that snapshot and are now published at 1.4.0. The public follow-up below is separate evidence. Prior 1.3.0 package evidence remains explicitly historical. The user-installed AGENTS framework is a development dependency; no NuGet package, SDK or CI action version was added or upgraded.

## Inventory scope and evidence

Framework integration review at `2026-10-10T01:47:53Z` (`2026-10-09 22:47:53 -03:00`, America/Sao_Paulo): all 17 maintained project declarations, four props files, three solutions, workflow and release settings were inspected against this inventory. The 23 existing local assets graphs still contain exactly the 73 documented package/version pairs, including 67 external pairs, and 79 distinct typed library identities; the documented-versus-observed package-pair difference is empty. All 119 distinct package cache directories referenced by those graphs retain their nuspec, cache provenance and any declared license file. Their recorded sources remain NuGet.org and the local `artifacts/packages` feed. The two existing public follow-up graphs contain respectively three and two Culture 1.4.0 packages, no project libraries, and only NuGet.org cache provenance. This is a reinspection of available evidence, not a new restore snapshot or a rerun of the 201 release tests. Source, dependency declarations and release workflow behavior are unchanged by the framework integration.

Publication follow-up on `2026-10-09 22:17:39 -03:00` (`2026-10-10T01:17:39Z`): the renamed-repository [tag workflow](https://github.com/Evigila/Essential/actions/runs/38011680225) passed OIDC login and six uploads. All six publicly indexed/downloaded 1.4.0 packages passed metadata/assets/dependency validation (`artifacts/culture-1.4-public-packages/verification.json`). Two additional fresh public-only consumer graphs, under `artifacts/public-consumers/culture-1.4.0-5db326106f3e4de4a2b53f17c91da2fe/{blazor,modules}/obj/project.assets.json`, contain no source-project libraries. Blazor/Core/Generator resolve exactly 1.4.0 with public NuGet.org cache provenance; both consumers built and ran, and the module consumer also published and ran. These two generated follow-up graphs do not alter the dated 23-graph inventory or its 73 package-pair counts. See [release verification](release-verification.md).

Verified at `2026-10-10T00:50:21Z` (`2026-10-09 21:50:21 -03:00`, America/Sao_Paulo). Baseline: `49556163d2e72fe8ffa27cf5f942149823844bb0` plus the authorized Culture 1.4.0 working tree. There are 17 maintained projects, including the optional performance console. The current snapshot covers 23 actual restore graphs: six libraries, four test suites, five source-mode Galleries, five isolated candidate-package Galleries, the performance console, the isolated Blazor fixture and a generated module consumer. These are 18 consuming project identities; the module consumer is temporary rather than a maintained project.

The local preparation closure contains 73 package IDs/version pairs: 67 external pairs identical to the pre-change audit and six Culture 1.4.0 candidate pairs. Assets also identify the same six source libraries as type `project`, giving 79 distinct `(type, ID, version)` library identities; package and source-project identities are counted separately. With no NuGet lock file or `global.json`, a future toolchain/feed can resolve differently. Declared constraints and observed versions remain separate. The evidence table below records the earlier preparation checks; their pre-publication statements describe that check's scope rather than the subsequent release outcome above.

| Evidence | Establishes | Result |
| --- | --- | --- |
| All 17 maintained `.csproj` files and four root/nested props | Package/project/framework declarations, current 1.4.0 module version and optional performance reference | Inspected; no external dependency change |
| `scripts/Test-Release.ps1 -ArtifactsPath artifacts/culture-1.4-final`; `artifacts/culture-1.4-final.log` | Root and source Gallery restore/build, all four test suites, six packages and isolated Blazor consumer | Passed: 201 tests (89/82/26/4); builds zero warnings/errors |
| Final Generator repack and `scripts/Verify-PackageSet.ps1` | Six current candidate packages; repository/project URLs, README, MIT license, exact internal version edges, managed XML docs and assets; buildTransitive asset SHA-256 matches source | Passed; no tag/push/publication |
| `artifacts/culture-1.4-final-modules.log` and latest PM graph below | Fresh Core-only package consumer after final target cleanup changes; transitive Generator, root/module keys, manifest deployment, policy and published execution | Passed, zero warnings/errors |
| `artifacts/culture-1.4-gallery-packages.log`, `artifacts/culture-1.4-gallery.NuGet.Config`, fresh `artifacts/culture-1.4-gallery-cache` | Five package-mode Gallery graphs/builds with `UseLocalCulture=false`; source mapping permits Culture packages only from the local candidate feed | Passed, zero warnings/errors; all six candidates actually resolved |
| `artifacts/performance-current-build/obj/Tests.Essential.Culture.Performance/project.assets.json` | Current optional tool's actual restore | Passed; no resolved NuGet package in P itself |
| Current `project.assets.json`, generated NuGet props/targets, cache nuspec/license/`.nupkg.metadata` | Exact versions, direct/transitive parents, framework/download dependencies and license/cache provenance | Inspected across all 23 current graphs |
| Focused dotnet/MSBuild/runtime/pack, Git and PowerShell queries; SDK `Microsoft.NETCoreSdk.BundledVersions.props` | Actual tool/framework versions and .NET 10 ref/apphost defaults | Versions recorded below; earlier full dotnet --info SCM permission failure did not prevent focused verification |
| Workflow/release settings/scripts and Generator buildTransitive files | CI/service/build/deployment/package verification contracts | Inspected; no publication executed |

### Current consumer aliases and exact resolution paths

| Alias | Consuming project/declaration | Resolution evidence |
| --- | --- | --- |
| C | `src/Essential.Culture/Essential.Culture/Essential.Culture.csproj` | `artifacts/culture-1.4-final/obj/Essential.Culture/project.assets.json` |
| G | `src/Essential.Culture/Essential.Culture.Generator/Essential.Culture.Generator.csproj` | `artifacts/culture-1.4-final/obj/Essential.Culture.Generator/project.assets.json` |
| A | `src/Essential.Culture/Essential.Culture.Avalonia/Essential.Culture.Avalonia.csproj` | `artifacts/culture-1.4-final/obj/Essential.Culture.Avalonia/project.assets.json` |
| W | `src/Essential.Culture/Essential.Culture.Wpf/Essential.Culture.Wpf.csproj` | `artifacts/culture-1.4-final/obj/Essential.Culture.Wpf/project.assets.json` |
| U | `src/Essential.Culture/Essential.Culture.WinUI/Essential.Culture.WinUI.csproj` | `artifacts/culture-1.4-final/obj/Essential.Culture.WinUI/project.assets.json` |
| B | `src/Essential.Culture/Essential.Culture.Blazor/Essential.Culture.Blazor.csproj` | `artifacts/culture-1.4-final/obj/Essential.Culture.Blazor/project.assets.json` |
| TC | `tests/Tests.Essential.Culture/Tests.Essential.Culture.csproj` | `artifacts/culture-1.4-final/obj/Tests.Essential.Culture/project.assets.json` |
| TG | `tests/Tests.Essential.Culture.Generator/Tests.Essential.Culture.Generator.csproj` | `artifacts/culture-1.4-final/obj/Tests.Essential.Culture.Generator/project.assets.json` |
| TU | `tests/Tests.Essential.Culture.WinUI/Tests.Essential.Culture.WinUI.csproj` | `artifacts/culture-1.4-final/obj/Tests.Essential.Culture.WinUI/project.assets.json` |
| TB | `tests/Tests.Essential.Culture.Blazor/Tests.Essential.Culture.Blazor.csproj` | `artifacts/culture-1.4-final/obj/Tests.Essential.Culture.Blazor/project.assets.json` |
| P | `tests/Tests.Essential.Culture.Performance/Tests.Essential.Culture.Performance.csproj` | `artifacts/performance-current-build/obj/Tests.Essential.Culture.Performance/project.assets.json` |
| PC | `tests/package-consumers/Tests.Essential.Culture.Blazor.PackageConsumer/Tests.Essential.Culture.Blazor.PackageConsumer.csproj (isolated copy)` | `artifacts/package-consumers/blazor-09a6b89c4c264c42926489a6592fb5ce/obj/project.assets.json` |
| PM | `Temporary module-only Core project generated by scripts/Verify-ModulesPackageConsumer.ps1` | `artifacts/package-consumers/modules-ba9b3d647636483599b625bca420d24c/obj/project.assets.json` |
| GC | `demo/Gallery.Essential.Culture/Gallery.Essential.Culture.csproj` | `artifacts/culture-1.4-final/obj/Gallery.Essential.Culture/project.assets.json`; `artifacts/culture-1.4-gallery-packages/obj/Gallery.Essential.Culture/project.assets.json` |
| GW | `demo/Gallery.Essential.Culture.Wpf/Gallery.Essential.Culture.Wpf.csproj` | `artifacts/culture-1.4-final/obj/Gallery.Essential.Culture.Wpf/project.assets.json`; `artifacts/culture-1.4-gallery-packages/obj/Gallery.Essential.Culture.Wpf/project.assets.json` |
| GA | `demo/Gallery.Essential.Culture.Avalonia/Gallery.Essential.Culture.Avalonia.csproj` | `artifacts/culture-1.4-final/obj/Gallery.Essential.Culture.Avalonia/project.assets.json`; `artifacts/culture-1.4-gallery-packages/obj/Gallery.Essential.Culture.Avalonia/project.assets.json` |
| GU | `demo/Gallery.Essential.Culture.WinUI/Gallery.Essential.Culture.WinUI.csproj` | `artifacts/culture-1.4-final/obj/Gallery.Essential.Culture.WinUI/project.assets.json`; `artifacts/culture-1.4-gallery-packages/obj/Gallery.Essential.Culture.WinUI/project.assets.json` |
| GB | `demo/Gallery.Essential.Culture.Blazor/Gallery.Essential.Culture.Blazor.csproj` | `artifacts/culture-1.4-final/obj/Gallery.Essential.Culture.Blazor/project.assets.json`; `artifacts/culture-1.4-gallery-packages/obj/Gallery.Essential.Culture.Blazor/project.assets.json` |

All source-mode graphs use NuGet.org for external dependencies. Candidate Gallery restore uses the explicit source-mapping config and a fresh cache; PC/PM restore from `artifacts/packages` into their own fresh caches. Source **N** means actual cache metadata records NuGet.org; **L** means it records the local `Essential/artifacts/packages` feed. N can describe an already populated global cache and does not imply every package was freshly downloaded. Every current nuspec license declaration was inspected; referenced license files are present. Legacy URLs/file licenses keep unknown SPDX identifiers explicit.

The ignored `artifacts/performance-baseline` project copies and their alternate restore outputs are comparison fixtures, not additional maintained or production dependencies.

## AGENTS framework development dependency

The user installed and authorized review, commit and push of this control framework. It affects instruction routing, documentation audits and optional synchronization; it is outside the production/package restore closure and is not loaded by any Culture runtime or adapter.

| Identity/kind | Declared and observed version/source | Relationship and consumers | Purpose, evidence and license |
| --- | --- | --- | --- |
| AGENTS framework; imported development instructions and scripts | `AGENTS.framework.json` declares `Evigila/AGENTS.md`, release `1.0.0`; `AGENTS.lock.json` records `https://github.com/Evigila/AGENTS.md.git`, ref `main`, exact imported commit `8f02f0f93a906c925d8f8a466887a7aed3c2b08a` | Direct repository development dependency; root router, shared standards, audit/synchronization skills and installer | Manifest has 17 payload entries: 10 managed files and 7 seed paths. License of the external framework is unknown; Essential's MIT package license is not evidence of that framework's license. Source/installation facts come from the local manifest, lock and imported files. |
| Project-owned audit and synchronization skill packages; development workflow resources | Imported with framework `1.0.0`; `.agents/skills/audit-project-docs/` and `.agents/skills/sync-agents-framework/`, each with `SKILL.md` and `agents/openai.yaml` | Framework-provided workflow resources consumed by the coding agent, with direct `SKILL.md` access when native discovery is unavailable | Root routing and `common/rules.md` supply mandatory standards; skills implement scoped workflows and do not depend on implicit matching. They add no runtime or NuGet dependency. |
| Existing Git, PowerShell and Windows batch host; installer execution dependencies | Unpinned Git; `scripts/fetch-agents.ps1` requires PowerShell `5.1` or later; batch launcher prefers `pwsh.exe` and falls back to `powershell.exe` | Direct tools for `fetch-agents.bat` / `scripts/fetch-agents.ps1`; same existing development tools enumerated below | Git resolves source content at one commit; PowerShell uses standard JSON, UTF-8 and SHA-256 APIs. No additional shell module or package is declared. Windows batch is optional when invoking the PowerShell installer directly. |

The lock's 11 managed baselines, including its manifest digest `ca5a058a2960b279eaf62d7fcf4c978ca064ff9a3e528485e8f0a315744432ee`, all match local content after CRLF-to-LF normalization and removal of an initial UTF-8 BOM. The installer validates the exact source commit and payload digests, retains existing seed/project facts and audit state, and limits managed writes to manifest-owned files. Audit state is separate from installation state; the coordinating full audit is required because the prior schema lacks the installed audited framework version. No network update is performed merely by reading these local dependency records.

## Direct package declarations

A bare PackageReference version is a NuGet minimum constraint, not an exact lock. Gallery package references are active when `UseLocalCulture != true`; current `VersionPrefix` is 1.4.0. PC uses `CulturePackageVersion`, default 1.4.0; PM receives it explicitly. The five package-mode Galleries and isolated consumers actually resolved prepared 1.4.0 packages from the candidate feed; this is local candidate evidence, not a claim of public publication.

| Package ID | Declared version/property | Direct consumers | Usage and purpose |
| --- | --- | --- | --- |
| `Avalonia` | 12.1.0 | A, GA | Production adapter/Gallery framework |
| `Avalonia.Desktop` | 12.1.0 | GA | Gallery desktop platform integration |
| `Avalonia.Themes.Fluent` | 12.1.0 | GA | Gallery theme |
| `Microsoft.WindowsAppSDK` | 2.3.1 | U, GU | WinUI framework/runtime/build components |
| `Microsoft.CodeAnalysis.CSharp` | 4.14.0, PrivateAssets=all | G, TG | Generator development/compiler tests; dependencies suppressed when packing Generator |
| `Microsoft.NET.Test.Sdk` | 17.14.1 | TC, TG, TB, TU | Test runner/build integration |
| `xunit` | 2.9.3 | TC, TG, TB, TU | Test assertions/execution |
| `xunit.runner.visualstudio` | 3.1.4, PrivateAssets=all | TC, TG, TB, TU | VSTest adapter |
| `Arkheide.Essential.Culture` | $(VersionPrefix)=1.4.0 | GC (package mode); generated module consumer at verification version | Console Gallery/runtime and module package verification |
| `Arkheide.Essential.Culture.Wpf` | $(VersionPrefix)=1.4.0 | GW (package mode) | WPF Gallery localization |
| `Arkheide.Essential.Culture.Avalonia` | $(VersionPrefix)=1.4.0 | GA (package mode) | Avalonia Gallery localization |
| `Arkheide.Essential.Culture.WinUI` | $(VersionPrefix)=1.4.0 | GU (package mode) | WinUI Gallery localization |
| `Arkheide.Essential.Culture.Blazor` | $(VersionPrefix)=1.4.0; $(CulturePackageVersion)=1.4.0 | GB (package mode), PC | Scoped Blazor localization |
| `Arkheide.Essential.Culture.Generator` | $(VersionPrefix)=1.4.0, PrivateAssets=all | GB (package mode) | Explicit Gallery generation; other package consumers receive it transitively |

## Complete current resolved package graph

The table enumerates all **73 current package/version pairs** in the 23 graphs above. D = explicit direct dependency; I = SDK auto-reference; T = resolved transitive dependency. Parent names come from target edges, including project targets that use their PackageId. PC and PM are isolated package consumers; P has no package row. Licenses and cache origins were checked in all observed current caches and agree.

| Package ID | Resolved version | Relationship/consumers | Introduced by | Usage scope and purpose | License/cache source |
| --- | --- | --- | --- | --- | --- |
| `Arkheide.Essential.Culture` | 1.4.0 | D: GC, PM; T: GA, GB, GU, GW, PC | Arkheide.Essential.Culture.Avalonia, Arkheide.Essential.Culture.Blazor, Arkheide.Essential.Culture.WinUI, Arkheide.Essential.Culture.Wpf | Test; Gallery development: Localization runtime and catalog/context API | MIT; source L |
| `Arkheide.Essential.Culture.Avalonia` | 1.4.0 | D: GA | Project PackageReference | Gallery development: Avalonia localization adapter | MIT; source L |
| `Arkheide.Essential.Culture.Blazor` | 1.4.0 | D: GB, PC | Project PackageReference | Test; Gallery development: Scoped Blazor localization | MIT; source L |
| `Arkheide.Essential.Culture.Generator` | 1.4.0 | D: GB; T: GA, GC, GU, GW, PC, PM | Arkheide.Essential.Culture | Test build; Gallery build: Transitive compile-time localization generator | MIT; source L |
| `Arkheide.Essential.Culture.WinUI` | 1.4.0 | D: GU | Project PackageReference | Gallery development: WinUI localization adapter | MIT; source L |
| `Arkheide.Essential.Culture.Wpf` | 1.4.0 | D: GW | Project PackageReference | Gallery development: WPF localization adapter | MIT; source L |
| `Avalonia` | 12.1.0 | D: A, GA; T: TC | Arkheide.Essential.Culture.Avalonia, Avalonia.Desktop, Avalonia.FreeDesktop, Avalonia.FreeDesktop.AtSpi, Avalonia.HarfBuzz, Avalonia.Native, Avalonia.Skia, Avalonia.Themes.Fluent, Avalonia.Win32, Avalonia.X11 | Production adapter/runtime; Test; Gallery development: Avalonia control/binding framework | MIT; source N |
| `Avalonia.Angle.Windows.Natives` | 2.1.27548.20260419 | T: GA | Avalonia.Win32 | Gallery development: Windows ANGLE native rendering backend | file `LICENSE` (present); SPDX unknown; source N |
| `Avalonia.BuildServices` | 11.3.2 | T: A, GA, TC | Avalonia | Adapter build; Test build; Gallery build: Avalonia build integration | MIT; source N |
| `Avalonia.Desktop` | 12.1.0 | D: GA | Project PackageReference | Gallery development: Desktop platform integration | MIT; source N |
| `Avalonia.FreeDesktop` | 12.1.0 | T: GA | Avalonia.X11 | Gallery development: Linux desktop integration | MIT; source N |
| `Avalonia.FreeDesktop.AtSpi` | 12.1.0 | T: GA | Avalonia.X11 | Gallery development: Linux accessibility integration | MIT; source N |
| `Avalonia.HarfBuzz` | 12.1.0 | T: GA | Avalonia.Desktop | Gallery development: Avalonia text shaping integration | MIT; source N |
| `Avalonia.Native` | 12.1.0 | T: GA | Avalonia.Desktop | Gallery development: Avalonia native macOS integration | MIT; source N |
| `Avalonia.Remote.Protocol` | 12.1.0 | T: A, GA, TC | Avalonia | Production adapter/runtime; Test; Gallery development: Avalonia remote protocol contract | MIT; source N |
| `Avalonia.Skia` | 12.1.0 | T: GA | Avalonia.Desktop, Avalonia.X11 | Gallery development: Avalonia Skia rendering integration | MIT; source N |
| `Avalonia.Themes.Fluent` | 12.1.0 | D: GA | Project PackageReference | Gallery development: Avalonia Gallery Fluent theme | MIT; source N |
| `Avalonia.Win32` | 12.1.0 | T: GA | Avalonia.Desktop | Gallery development: Windows desktop integration | MIT; source N |
| `Avalonia.X11` | 12.1.0 | T: GA | Avalonia.Desktop | Gallery development: X11 desktop integration | MIT; source N |
| `HarfBuzzSharp` | 8.3.1.3 | T: GA | Avalonia.HarfBuzz, Avalonia.Skia | Gallery development: Text shaping or platform-native assets | MIT; source N |
| `HarfBuzzSharp.NativeAssets.Linux` | 8.3.1.3 | T: GA | Avalonia.HarfBuzz, Avalonia.Skia | Gallery development: Text shaping or platform-native assets | MIT; source N |
| `HarfBuzzSharp.NativeAssets.macOS` | 8.3.1.3 | T: GA | HarfBuzzSharp | Gallery development: Text shaping or platform-native assets | MIT; source N |
| `HarfBuzzSharp.NativeAssets.WebAssembly` | 8.3.1.3 | T: GA | Avalonia.HarfBuzz, Avalonia.Skia | Gallery development: Text shaping or platform-native assets | MIT; source N |
| `HarfBuzzSharp.NativeAssets.Win32` | 8.3.1.3 | T: GA | HarfBuzzSharp | Gallery development: Text shaping or platform-native assets | MIT; source N |
| `MicroCom.Runtime` | 0.11.6 | T: A, GA, TC | Avalonia | Production adapter/runtime; Test; Gallery development: COM/native interop runtime | MIT; source N |
| `Microsoft.AspNetCore.App.Internal.Assets` | 10.0.12 | I: GB | SDK auto-reference | Gallery build: Web SDK static asset build integration | MIT; source N |
| `Microsoft.CodeAnalysis.Analyzers` | 3.11.0 | T: G, TG | Microsoft.CodeAnalysis.Common, Microsoft.CodeAnalysis.CSharp | Generator build; Test build: Roslyn analyzer development checks | MIT; source N |
| `Microsoft.CodeAnalysis.Common` | 4.14.0 | T: G, TG | Microsoft.CodeAnalysis.CSharp | Generator build; Test: Roslyn compiler/generator APIs | MIT; source N |
| `Microsoft.CodeAnalysis.CSharp` | 4.14.0 | D: G, TG | Project PackageReference | Generator build; Test: Roslyn compiler/generator APIs | MIT; source N |
| `Microsoft.CodeCoverage` | 17.14.1 | T: TB, TC, TG, TU | Microsoft.NET.Test.Sdk | Test: Test SDK coverage infrastructure | MIT; source N |
| `Microsoft.NET.Test.Sdk` | 17.14.1 | D: TB, TC, TG, TU | Project PackageReference | Test: Test execution/build integration | MIT; source N |
| `Microsoft.NETCore.Platforms` | 1.1.0 | T: G | NETStandard.Library | Generator build: .NET Standard platform reference dependency | [legacy license URL](http://go.microsoft.com/fwlink/?LinkId=329770); SPDX unknown; source N |
| `Microsoft.TestPlatform.ObjectModel` | 17.14.1 | T: TB, TC, TG, TU | Microsoft.TestPlatform.TestHost | Test: Test host/object model infrastructure | MIT; source N |
| `Microsoft.TestPlatform.TestHost` | 17.14.1 | T: TB, TC, TG, TU | Microsoft.NET.Test.Sdk | Test: Test host/object model infrastructure | MIT; source N |
| `Microsoft.Web.WebView2` | 1.0.3719.77 | T: GU, TU, U | Microsoft.WindowsAppSDK.WinUI | Production adapter/runtime; Test; Gallery development: Windows App SDK WebView2 integration | file `LICENSE.txt` (present); SPDX unknown; source N |
| `Microsoft.Windows.AI.MachineLearning` | 2.1.74 | T: GU, TU, U | Microsoft.WindowsAppSDK.ML | Production adapter/runtime; Test; Gallery development: Windows App SDK ML runtime component | file `license.txt` (present); SPDX unknown; source N |
| `Microsoft.Windows.SDK.BuildTools` | 10.0.26100.4654 | T: GU, TU, U | Microsoft.WindowsAppSDK.Base | Adapter build; Test build; Gallery build: Windows SDK build tools | [legacy license URL](https://aka.ms/WinSDKLicenseURL); SPDX unknown; source N |
| `Microsoft.Windows.SDK.BuildTools.MSIX` | 1.7.251221100 | T: GU, TU, U | Microsoft.WindowsAppSDK.Base | Adapter build; Test build; Gallery build: MSIX build/packaging tools | file `sdk_license.txt` (present); SPDX unknown; source N |
| `Microsoft.WindowsAppSDK` | 2.3.1 | D: GU, U; T: TU | Arkheide.Essential.Culture.WinUI | Production adapter/runtime; Test; Gallery development: WinUI/Windows App SDK aggregate | file `license.txt` (present); SPDX unknown; source N |
| `Microsoft.WindowsAppSDK.AI` | 2.3.4 | T: GU, TU, U | Microsoft.WindowsAppSDK | Production adapter/runtime; Test; Gallery development: Resolved Windows App SDK component: AI | file `license.txt` (present); SPDX unknown; source N |
| `Microsoft.WindowsAppSDK.Base` | 2.0.4 | T: GU, TU, U | Microsoft.WindowsAppSDK, Microsoft.WindowsAppSDK.AI, Microsoft.WindowsAppSDK.DWrite, Microsoft.WindowsAppSDK.Foundation, Microsoft.WindowsAppSDK.InteractiveExperiences, Microsoft.WindowsAppSDK.ML, Microsoft.WindowsAppSDK.Runtime, Microsoft.WindowsAppSDK.Widgets, Microsoft.WindowsAppSDK.WinUI | Production adapter/runtime; Test; Gallery development: Resolved Windows App SDK component: Base | file `license.txt` (present); SPDX unknown; source N |
| `Microsoft.WindowsAppSDK.DWrite` | 2.1.0 | T: GU, TU, U | Microsoft.WindowsAppSDK | Production adapter/runtime; Test; Gallery development: Resolved Windows App SDK component: DWrite | file `license.txt` (present); SPDX unknown; source N |
| `Microsoft.WindowsAppSDK.Foundation` | 2.3.5 | T: GU, TU, U | Microsoft.WindowsAppSDK, Microsoft.WindowsAppSDK.AI, Microsoft.WindowsAppSDK.ML, Microsoft.WindowsAppSDK.WinUI | Production adapter/runtime; Test; Gallery development: Resolved Windows App SDK component: Foundation | file `license.txt` (present); SPDX unknown; source N |
| `Microsoft.WindowsAppSDK.InteractiveExperiences` | 2.1.3 | T: GU, TU, U | Microsoft.WindowsAppSDK, Microsoft.WindowsAppSDK.Foundation, Microsoft.WindowsAppSDK.WinUI | Production adapter/runtime; Test; Gallery development: Resolved Windows App SDK component: InteractiveExperiences | file `license.txt` (present); SPDX unknown; source N |
| `Microsoft.WindowsAppSDK.ML` | 2.1.74 | T: GU, TU, U | Microsoft.WindowsAppSDK | Production adapter/runtime; Test; Gallery development: Resolved Windows App SDK component: ML | file `license.txt` (present); SPDX unknown; source N |
| `Microsoft.WindowsAppSDK.Runtime` | 2.3.1 | T: GU, TU, U | Microsoft.WindowsAppSDK | Production adapter/runtime; Test; Gallery development: Resolved Windows App SDK component: Runtime | file `license.txt` (present); SPDX unknown; source N |
| `Microsoft.WindowsAppSDK.Widgets` | 2.0.5 | T: GU, TU, U | Microsoft.WindowsAppSDK | Production adapter/runtime; Test; Gallery development: Resolved Windows App SDK component: Widgets | file `license.txt` (present); SPDX unknown; source N |
| `Microsoft.WindowsAppSDK.WinUI` | 2.3.0 | T: GU, TU, U | Microsoft.WindowsAppSDK | Production adapter/runtime; Test; Gallery development: Resolved Windows App SDK component: WinUI | file `license.txt` (present); SPDX unknown; source N |
| `NETStandard.Library` | 2.0.3 | I: G | SDK auto-reference | Generator build: .NET Standard 2.0 compile reference package | [legacy license URL](https://github.com/dotnet/standard/blob/master/LICENSE.TXT); SPDX unknown; source N |
| `Newtonsoft.Json` | 13.0.3 | T: TB, TC, TG, TU | Microsoft.TestPlatform.TestHost | Test: Test host JSON support; not Core JSON parser | MIT; source N |
| `SkiaSharp` | 3.119.4 | T: GA | Avalonia.Skia | Gallery development: 2D renderer or platform-native assets | MIT; source N |
| `SkiaSharp.NativeAssets.Linux` | 3.119.4 | T: GA | Avalonia.Skia | Gallery development: 2D renderer or platform-native assets | MIT; source N |
| `SkiaSharp.NativeAssets.macOS` | 3.119.4 | T: GA | SkiaSharp | Gallery development: 2D renderer or platform-native assets | MIT; source N |
| `SkiaSharp.NativeAssets.WebAssembly` | 3.119.4 | T: GA | Avalonia.Skia | Gallery development: 2D renderer or platform-native assets | MIT; source N |
| `SkiaSharp.NativeAssets.Win32` | 3.119.4 | T: GA | SkiaSharp | Gallery development: 2D renderer or platform-native assets | MIT; source N |
| `System.Buffers` | 4.5.1 | T: G | Microsoft.CodeAnalysis.Common, Microsoft.CodeAnalysis.CSharp, System.Memory | Generator build: Roslyn/.NET Standard compatibility library | [legacy license URL](https://github.com/dotnet/corefx/blob/master/LICENSE.TXT); SPDX unknown; source N |
| `System.Collections.Immutable` | 9.0.0 | T: G | Microsoft.CodeAnalysis.Common, Microsoft.CodeAnalysis.CSharp, System.Reflection.Metadata | Generator build: Roslyn/.NET Standard compatibility library | MIT; source N |
| `System.Memory` | 4.5.5 | T: G | Microsoft.CodeAnalysis.Common, Microsoft.CodeAnalysis.CSharp, System.Collections.Immutable, System.Reflection.Metadata, System.Text.Encoding.CodePages | Generator build: Roslyn/.NET Standard compatibility library | [legacy license URL](https://github.com/dotnet/corefx/blob/master/LICENSE.TXT); SPDX unknown; source N |
| `System.Numerics.Tensors` | 9.0.0 | T: GU, TU, U | Microsoft.Windows.AI.MachineLearning | Production adapter/runtime; Test; Gallery development: Windows ML tensor support | MIT; source N |
| `System.Numerics.Vectors` | 4.5.0 | T: G | Microsoft.CodeAnalysis.Common, Microsoft.CodeAnalysis.CSharp, System.Memory | Generator build: Roslyn/.NET Standard compatibility library | [legacy license URL](https://github.com/dotnet/corefx/blob/master/LICENSE.TXT); SPDX unknown; source N |
| `System.Reflection.Metadata` | 9.0.0 | T: G | Microsoft.CodeAnalysis.Common, Microsoft.CodeAnalysis.CSharp | Generator build: Roslyn/.NET Standard compatibility library | MIT; source N |
| `System.Runtime.CompilerServices.Unsafe` | 6.0.0 | T: G | Microsoft.CodeAnalysis.Common, Microsoft.CodeAnalysis.CSharp, System.Collections.Immutable, System.Memory, System.Text.Encoding.CodePages, System.Threading.Tasks.Extensions | Generator build: Roslyn/.NET Standard compatibility library | MIT; source N |
| `System.Text.Encoding.CodePages` | 7.0.0 | T: G | Microsoft.CodeAnalysis.Common, Microsoft.CodeAnalysis.CSharp | Generator build: Roslyn/.NET Standard compatibility library | MIT; source N |
| `System.Threading.Tasks.Extensions` | 4.5.4 | T: G | Microsoft.CodeAnalysis.Common, Microsoft.CodeAnalysis.CSharp | Generator build: Roslyn/.NET Standard compatibility library | [legacy license URL](https://github.com/dotnet/corefx/blob/master/LICENSE.TXT); SPDX unknown; source N |
| `Tmds.DBus.Protocol` | 0.94.1 | T: GA | Avalonia.FreeDesktop | Gallery development: Linux D-Bus protocol integration | MIT; source N |
| `xunit` | 2.9.3 | D: TB, TC, TG, TU | Project PackageReference | Test: xUnit assertions/test execution contracts | Apache-2.0; source N |
| `xunit.abstractions` | 2.0.3 | T: TB, TC, TG, TU | xunit.extensibility.core | Test: xUnit assertions/test execution contracts | [legacy license URL](https://raw.githubusercontent.com/xunit/xunit/master/license.txt); SPDX unknown; source N |
| `xunit.analyzers` | 1.18.0 | T: TB, TC, TG, TU | xunit | Test: Test source analyzers | Apache-2.0; source N |
| `xunit.assert` | 2.9.3 | T: TB, TC, TG, TU | xunit | Test: xUnit assertions/test execution contracts | Apache-2.0; source N |
| `xunit.core` | 2.9.3 | T: TB, TC, TG, TU | xunit | Test: xUnit assertions/test execution contracts | Apache-2.0; source N |
| `xunit.extensibility.core` | 2.9.3 | T: TB, TC, TG, TU | xunit.core, xunit.extensibility.execution | Test: xUnit assertions/test execution contracts | Apache-2.0; source N |
| `xunit.extensibility.execution` | 2.9.3 | T: TB, TC, TG, TU | xunit.core | Test: xUnit assertions/test execution contracts | Apache-2.0; source N |
| `xunit.runner.visualstudio` | 3.1.4 | D: TB, TC, TG, TU | Project PackageReference | Test: VSTest xUnit adapter | Apache-2.0; source N |

### Historical 1.3.0 package evidence

The pre-change forced audit successfully restored 21 graphs under `artifacts/documentation-audit/obj/` (six libraries, four tests, five source Galleries and the Blazor consumer) and `artifacts/documentation-audit-packages/obj/` (five package Galleries). Those SDK 10.0.401 commands used `MSBuildEnableWorkloadResolver=false`, `DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1` and `DOTNET_NOLOGO=1`; approved execution succeeded after sandbox restore failures. These are historical outcomes, not current 1.4.0 package verification.

The preceding closure also had 73 pairs. Its 67 external pairs are unchanged and fully enumerated once in the current table; the six distinct old Culture pairs are retained below. In this historical table, aliases refer to the earlier graphs only: PC was the 1.3.0 fixture; Gallery aliases use `artifacts/documentation-audit-packages/obj/<project basename>/project.assets.json`. Historical N/L sources describe the earlier cache provenance, including some reused local 1.3.0 packages, and make no fresh-public-only claim.

| Package ID | Historical resolved version | Relationship/historical consumers | Introduced by | Historical use | License/cache source |
| --- | --- | --- | --- | --- | --- |
| `Arkheide.Essential.Culture.Avalonia` | 1.3.0 | D: GA | Project PackageReference | Gallery development: Avalonia localization adapter | MIT; source N |
| `Arkheide.Essential.Culture.Blazor` | 1.3.0 | D: GB, PC | Project PackageReference | Test; Gallery development: Scoped Blazor localization | MIT; source L |
| `Arkheide.Essential.Culture.Generator` | 1.3.0 | D: GB; T: GA, GC, GU, GW, PC | Arkheide.Essential.Culture | Test; Gallery build: Transitive compile-time localization generator | MIT; source L |
| `Arkheide.Essential.Culture.WinUI` | 1.3.0 | D: GU | Project PackageReference | Gallery development: WinUI localization adapter | MIT; source N |
| `Arkheide.Essential.Culture.Wpf` | 1.3.0 | D: GW | Project PackageReference | Gallery development: WPF localization adapter | MIT; source L |
| `Arkheide.Essential.Culture` | 1.3.0 | D: GC; T: GA, GB, GU, GW, PC | Arkheide.Essential.Culture.Avalonia, Arkheide.Essential.Culture.Blazor, Arkheide.Essential.Culture.WinUI, Arkheide.Essential.Culture.Wpf | Test; Gallery development: Localization runtime and catalog/context API | MIT; source L |

## Local project references

| Consumer | Referenced project(s) | Purpose/condition/declaration |
| --- | --- | --- |
| C | G | Analyzer reference, ExcludeAssets=compile;runtime, Private=false, PrivateAssets=none; Core csproj |
| A, W, U, B | C | Runtime API; each adapter csproj |
| TC | C, A, W, G | Runtime/framework tests and explicit Generator analyzer reference; TC csproj |
| TG | G | Normal generator assembly access; TG csproj |
| TB | B | Scoped/component tests; TB csproj |
| TU | U | WinUI marker tests; TU csproj |
| GC | C, G | UseLocalCulture=true only; G is Analyzer/ReferenceOutputAssembly=false; GC csproj |
| GA | A, G | Same conditional analyzer/runtime source mode; GA csproj |
| GW | W, G | Same conditional analyzer/runtime source mode; GW csproj |
| GU | U, G | Same conditional analyzer/runtime source mode; GU csproj |
| GB | B, G | Same conditional analyzer/runtime source mode; GB csproj |
| PC, PM | None | Package-only fixture/generated consumer; no local project references |
| P | C by default; `BaselineCatalogProject` when explicitly supplied | Development-only timing/allocation comparison; its csproj has no direct external package and is outside release solutions |

Core/Generator remain framework-independent; adapters point inward to Core. No Essential project references Flourish. The current declared and resolved packaged graph is Adapter -> Core 1.4.0 -> Generator 1.4.0; every adapter was resolved in package Gallery mode. Prior 1.3.0 resolution remains in its historical table above. Generator compiler dependencies remain build-private/suppressed. The module consumer is generated by the release script in ignored artifacts rather than maintained as another project.

## SDKs, frameworks, and runtimes

| Dependency | Declared constraint / observed version | Consumers/scope | Purpose/evidence/license |
| --- | --- | --- | --- |
| .NET SDK / MSBuild | CI `10.0.x`; locally SDK 10.0.401 (10.0.301 also installed), MSBuild 18.9.11.42413; no global.json | All, build/test/development | Microsoft.NET.Sdk, Razor and Web project SDKs; workflow and dotnet queries; installed `C:/Program Files/dotnet/LICENSE.txt` declares Microsoft .NET Library terms; SPDX unknown |
| Microsoft.NETCore.App + reference/apphost packs | Target net10.0 family; SDK-selected 10.0.12 ref/apphost, local runtime 10.0.12 | All non-generator runtime/test/Gallery projects | .NET 10 runtime/build base; installed packs and generated framework references; no pinned runtime patch |
| .NET Standard library | netstandard2.0; SDK auto-reference NETStandard.Library 2.0.3 | G, build | Analyzer target framework; complete package dependencies listed above |
| Microsoft.WindowsDesktop.App.WPF/ref pack | net10.0-windows; SDK-selected ref and installed runtime 10.0.12 | W, GW, TC | WPF bindings/hosts/tests; UseWPF and framework references |
| Microsoft.AspNetCore.App/ref pack | net10.0, SDK-selected ref and installed runtime 10.0.12 | B, GB, TB, PC | DI/components, SSR and Interactive Server; explicit or Web-SDK implicit framework references |
| Microsoft.Windows.SDK.NET.Ref | DownloadDependency exact [10.0.19041.57,10.0.19041.57] | U, GU, TU; build | WinRT reference pack, resolved project.frameworks downloadDependencies and generated framework reference; download-only pack is outside libraries table |
| Windows target/platform | Target windows10.0.19041.0; library/Gallery TargetPlatformMinVersion=10.0.17763.0; GU RID win-x64 | U, GU, TU | Source Windows API target; project files; target/min properties are not proof every deployment environment was tested |
| Windows App SDK runtime/native assets | Aggregate 2.3.1 and exact component versions above; GU self-contained/unpackaged | U, GU, TU | WinUI runtime/build closure. W/U operating-system runtime licenses are platform contracts, not NuGet SPDX assumptions |

Windows SDK reference pack license/cache source: [legacy license URL](https://aka.ms/WinSDKLicenseURL); SPDX unknown; source N

## External services

| Service | Contract/version | Scope/consumers | Purpose/evidence |
| --- | --- | --- | --- |
| NuGet.org | v3 service index; api.nuget.org/v3/index.json | Build/development restore; release publishing | Current restore source and workflow NUGET_SOURCE; cached origins separately marked above; version not applicable to service |
| GitHub repository and Actions | Evigila/Essential; build.yml; windows-latest runner; master/v*.*.* triggers; environment nuget | CI/release | Current origin, package URLs and TrustedPublishing settings/preflight; actual runner image/service version unpinned |
| GitHub framework source repository | Evigila/AGENTS.md; installed release 1.0.0 and imported commit recorded above; Git transport rather than a runtime API | Optional development synchronization | `fetch-agents.bat`, `scripts/fetch-agents.ps1`, framework manifest and lock; remote availability is required for a network check/update, while `-SourcePath` supports an existing local Git source. Service version is not applicable. |
| NuGet Trusted Publishing / GitHub OIDC | NuGet/login@v1; id-token:write | Tag release publishing | Workflow authentication contract; no stored credentials documented |
| Runtime translation backend/database | None declared | Production Core/adapters | Catalogs are supplied via JSON file/string/stream or in-memory registration; no external localization API/database dependency found |

## Assets and build/test/development tools

| Dependency | Version/constraint | Scope/consumers | Purpose/source/license/evidence |
| --- | --- | --- | --- |
| PowerShell (pwsh) | Observed 7.6.5; workflow uses pwsh, no exact repository pin; framework installer minimum PowerShell 5.1 | Build/test/release and optional framework synchronization | Script execution; PSVersionTable, existing scripts and installer `#requires`; distribution license not independently checked |
| Windows cmd.exe / batch host | OS-supplied, unpinned | Local publish-helper.bat and fetch-agents.bat | Starts PowerShell helpers; optional when invoking PS directly; OS license/version not independently checked |
| Git CLI | Observed 2.56.0.windows.1, unpinned | Development/release ancestry checks and optional framework source resolution | git --version, Release-Common.ps1 and fetch-agents.ps1; installed-distribution license not independently checked |
| NuGet CLI/restore tooling bundled in dotnet | Observed NuGetToolVersion 7.0.0 in generated restore props, SDK 10.0.401; no separate nuget.exe requirement | Restore/pack/publish | scripts/workflow use dotnet restore/pack/nuget push; generated csproj.nuget.g.props |
| actions/checkout | @v7 | CI | Checkout; .github/workflows/build.yml; tag constraint, exact commit/license not independently verified |
| actions/setup-dotnet | @v5 | CI | .NET 10 install; workflow; tag constraint, exact commit/license not independently verified |
| actions/upload-artifact | @v7 | Tag CI | Upload package set; workflow; tag constraint, exact commit/license not independently verified |
| actions/download-artifact | @v8 | Publish CI | Download verified package set; workflow; tag constraint, exact commit/license not independently verified |
| NuGet/login | @v1 | Publish CI | OIDC Trusted Publishing; workflow; tag constraint, exact commit/license not independently verified |
| Roslyn generator analyzer/buildTransitive assets | Current module 1.4.0 | Compilation/packaged consumers | Analyzer DLL, props/targets/template; module item metadata and bounded root discovery, safe deployment validation/copying/stale-output cleanup; inline RoslynCodeTaskFactory tasks use the existing SDK Microsoft.Build.Tasks.Core assembly; repository MIT; generator project/source |
| Avalonia Fluent theme/native rendering assets | Versions fully enumerated above | GA | NuGet-resolved theme/native dependencies, no external font download declaration |
| Browser/system fonts | system-ui, sans-serif, ui-monospace, monospace; no package version | GB runtime display | app.css references platform fonts; no checked-in/downloaded webfont or icon library |
| Culture JSON resources and local CSS | Maintained source, module 1.4.0 | Tests/Galleries/build template | Catalog JSON/app.css are project assets, not external packages; no third-party icon/image/font asset found |
| Standalone performance probe | net10.0; standard Stopwatch/GC/process APIs; no benchmark package | Optional development tool | Tests.Essential.Culture.Performance project/source; current Core or explicitly supplied baseline project; never added to standard solution/release test loop |
| Browser | No declared browser/version pin | Manual Blazor verification | Human acceptance tool; no Computer Use testing performed |

No active Node/JavaScript test dependency is configured: ConsoleTests, JavaScriptTests and CheckScripts arrays are empty. The release helper contains optional branches, which do not make Node an active project requirement. Visual Studio is optional; no IDE version requirement is declared. No DocFX dependency is currently declared merely by retaining docs/.

## Active constraints and unresolved facts

- Current declarations and local candidate resolution are 1.4.0; six historical rows preserve the pre-change 1.3.0 package results. No central package file, NuGet lock file, root NuGet.Config or SDK pin exists in the maintained repository. The separate AGENTS installation lock pins imported framework provenance, not application dependencies.
- The framework's imported release/version/commit and local normalized baselines are verified. Its external license remains unknown; the installed source ref `main` is moving, while the lock's commit identifies the already imported content.
- Unknown tool/action SPDX licenses and unpinned CI action commits/runner image are explicit boundaries. Cached package license-file references were checked for presence; legacy license URLs are metadata, not remotely reverified license text.
- Current standards require applicable Flourish UI controls, but no Flourish dependency is adopted by Essential. Gallery UI compliance is unresolved; adding a package/control or changing an SDK requires the applicable authorization and actual contract evidence.
- Gallery README package snippets still show 1.2.0 while current project declarations/version props are 1.4.0. Human documentation updates belong to the coordinating task; this inventory does not rewrite those files.
- Module composition, enabled/disabled language policy and performance probing use existing runtime/JSON/tooling APIs. No new external dependency is introduced. Future hot reload or external module distribution is outside the adopted scope.

Follow [external dependency authorization](../common/rules.md#external-dependencies) before changing declarations. An inventory row records usage and grants no adoption/migration permission.
