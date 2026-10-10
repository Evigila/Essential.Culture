# Culture NuGet release and integration

## Current candidate and repository rename

The working tree prepares **1.4.0** for the six existing Culture package IDs. Package metadata and release preflight now identify `https://github.com/Evigila/Essential`. The user recreated the NuGet Trusted Publishing policy after the repository rename. This task prepares and verifies local candidates; it does not create a commit/tag, push, or publish.

The effective policy fields are repository owner `Evigila`, repository name `Essential`, workflow filename `build.yml` and GitHub environment `nuget`. `NUGET_USER` remains a variable/secret containing the NuGet profile username. Preflight checks repository/workflow/environment/tag/OIDC context, while the remote account policy is verified only by a successful `NuGet/login` in an authorized tag workflow. Prior success under the old repository name is historical evidence and does not validate the recreated policy.

Preparation verifies six packages' identities, versions, dependency graph, repository/project URLs, README bytes, license, DLL/XML documentation and generator assets. It also executes separate fresh-cache candidate consumers for Blazor-only dependency adoption and core-only modular resource generation/build/publish. [Resource configuration](culture-resource-usage.md) documents the startup language policy and eager modules. [Release verification](release-verification.md) records actual results.

## Historical completed 1.3.0 release

Essential.Culture 1.3.0 is published, publicly indexed and verified by fresh-cache public-only consumption. Source commit [983c636bdba7e1a7b74cee33b5c682c9bf06364e](https://github.com/Evigila/Essential.Culture/commit/983c636bdba7e1a7b74cee33b5c682c9bf06364e) is tagged v1.3.0. [Run 37537838178](https://github.com/Evigila/Essential.Culture/actions/runs/37537838178) succeeded, including Trusted Publishing login and six Created uploads. All six public flat-container indexes returned HTTP 200 and contain 1.3.0.

The public-source-only consumer at artifacts/public-consumers/blazor-1e4f292745bf4a2fbe68954d77b4132a restored into a fresh cache, built with warnings as errors and ran successfully. Its only direct package is Arkheide.Essential.Culture.Blazor; Core and Generator restore transitively. Generated keys, independent scoped languages, formatting culture and encoded rendering passed.

Flourish 1.1.1 completed its GitHub release workflow and uploaded all six packages. Source commit [05ebb4d282eea7130fc9326ef2d15bf43593d5aa](https://github.com/Evigila/Flourish/commit/05ebb4d282eea7130fc9326ef2d15bf43593d5aa) is tagged v1.1.1. [Run 37540483235](https://github.com/Evigila/Flourish/actions/runs/37540483235) completed successfully: [build job 112531999414](https://github.com/Evigila/Flourish/actions/runs/37540483235/job/112531999414) passed, including the corrected CSS fixture; [publish job 112533073982](https://github.com/Evigila/Flourish/actions/runs/37540483235/job/112533073982) completed Trusted Publishing login and uploaded the six packages in manifest order with Created responses from 22:28:55 to 22:29:01 UTC on 2026-10-06.

All six Flourish 1.1.1 public flat-container indexes now return HTTP 200 and contain 1.1.1. Fresh-cache, public-only verification completed with exit code 0 across FrameworkOnly, MetaNative, MetaDesign and MetaCulture: all 129 checks passed, covering DI activation, SSR, CSS/woff2/JavaScript assets, three languages and generated keys. Evidence: Flourish/artifacts/public-culture-release-1.1.1.log and Flourish/artifacts/package-consumers/027abe99e60f46dbaa1d15a75b670ff6. Its NuGet.Config contains one public NuGet.org source with a wildcard mapping. Every .nupkg.metadata for the six Flourish 1.1.1 packages and Essential.Blazor/Core/Generator 1.3.0 identifies https://api.nuget.org/v3/index.json; no local feed or source project reference is used. Essential's independent public-only consumer also passed. Publication, public indexing and public consumption are complete for both releases.

## Module and package boundaries

Essential.slnx contains the Culture libraries and tests. The focused source solution is src/Essential.Culture/Essential.Culture.slnx; demo/Gallery.Essential.Culture.slnx contains five examples. Candidate VersionPrefix=1.4.0 belongs to src/Essential.Culture/Directory.Build.props. scripts/ReleaseSettings.psd1 releases in this order:

| Order | Essential.Culture 1.4.0 candidate package |
|---|---|
| 1 | Arkheide.Essential.Culture.Generator |
| 2 | Arkheide.Essential.Culture |
| 3 | Arkheide.Essential.Culture.Wpf |
| 4 | Arkheide.Essential.Culture.Avalonia |
| 5 | Arkheide.Essential.Culture.WinUI |
| 6 | Arkheide.Essential.Culture.Blazor |

Generator has no runtime dependency. Core owns immutable catalogs, contexts and the established desktop facade; the platform adapters depend on Core. The Blazor package brings Core/Generator transitively. Essential does not depend on Flourish. Stable Arkheide.* NuGet IDs are independent of the current Evigila/Essential GitHub repository identity.

Flourish's six 1.1.1 packages are Core, Abstract, Culture bridge, Framework, Design and the dependency-only Blazor umbrella, in that order. The umbrella installs its Culture bridge automatically; Framework alone omits Design/Culture. Flourish WPF is excluded. See the coordinated [Flourish release guide](../../../Flourish/docs-ai/current/nuget-release-integration.md) for exact IDs and the dependency graph.

## Single-file translations and Blazor behavior

Maintain the multilingual key format within each application-owned file. A single Culture.json remains the default; optional CultureModule items compose functional files into one logical catalog. Use the generated deployment manifest with FromFiles, or explicitly embed/load resources in the host. The Blazor consumer verifies Generator arrives transitively. Keep placeholder indices consistent, include fallback in every entry, and maintain uniform per-key culture sets within each file.

AddCultureBlazor registers immutable catalogs and a request/circuit-scoped ILocalizationService. AddCatalog, SetDefaultCatalog, SetDefaultCulture and AddSupportedCultures select catalogs/defaults; InitializeWith controls initial scope selection. Resolve generated string tokens with Parse/ParseFrom or TryParse/TryParseFrom. Generator emits a static Key class; TextKey is a host alias for that class, not a runtime type. The actual ILocalizationService APIs accept string tokens. Catalog IDs distinguish host and library dictionaries. Culture is a UI-culture string and FormatCulture is a formatting CultureInfo; LocalizedText encodes its output. LocalizedComponentBase reacts to Changed and disposes the subscription. Resolve visible prose/status/validation at render time instead of caching translated sentences.

The lookup/generation contract is shared with Essential's other targets. Blazor's lifetime and host integration differ: it does not use the process-wide desktop Localizer facade, change global ambient culture or persist a cookie itself. The host owns negotiation, culture endpoints, cookies and initial selection. SSR and Interactive Server are verified; standalone browser WebAssembly is not verified. Flourish's bridge adapts this scoped service to its neutral text provider, while Framework owns a separate embedded standard catalog.

## Local preparation

```powershell
# Run from the Essential repository root.
.\scripts\Test-Release.ps1 -ArtifactsPath .\artifacts\release-build
```

Prepare is the default of the batch helper. Test-Release restores/builds in Release with warnings as errors, tests the module, builds five source-mode Galleries, packs/verifies six libraries and checks isolated Blazor-only and modular core-only package consumers. ArtifactsPath isolates both SDK output and intermediate files. Demos otherwise default to PackageReference; a source-mode demo alone is not package-adoption evidence. -VerifyOnly and helper -SkipBuild inspect existing packages without proving they match current source, and do not waive Publish's guards.

The current 1.4.0 preparation passed 201 tests: Core 89, Generator 82, Blazor 26 and WinUI 4. Five Galleries built in source and candidate-package modes with zero warnings/errors. Six candidates and both isolated consumers passed; the final Generator build assets were checked against current source hashes. See [current verification](release-verification.md#current-local-140-candidate-verification-on-2026-10-09). The earlier 106-test result and public-only consumer belong to the historical 1.3.0 release.

For future unpublished Essential candidates, Flourish accepts an explicit EssentialPackageDirectory NuGet feed; it has no implicit sibling feed or source fallback. Flourish packs its targeted six-library graph before fresh-cache Gallery/full-solution validation and four isolated consumers. The current Essential 1.3.0 dependency is already available publicly. Flourish's explicit PublicSource mode also completed the four public-only consumers with 129 passing checks; it rejects local package-directory parameters and does not affect the default local/CI path.

## Trusted Publishing configuration

Both repositories use .github/workflows/build.yml and the GitHub environment nuget. Tag-only publish jobs require a successful build and permissions contents: read plus id-token: write. NuGet/login@v1 reads vars.NUGET_USER || secrets.NUGET_USER: an Actions Variable takes priority, otherwise the existing Secret is used. NUGET_USER is the NuGet.org profile username; the user confirmed Evigila. The username is not hard-coded. Login supplies a temporary NUGET_API_KEY for dotnet nuget push; no long-lived local API key, extra nuget.exe or GitHub CLI is required.

| Policy field | Essential | Flourish |
|---|---|---|
| Owner | Evigila | Evigila |
| Repository | Essential | Flourish |
| Repository ID | 1327295083 | 1246107902 |
| Workflow filename | build.yml | build.yml |
| Environment | nuget | nuget |

The workflow field is build.yml without .github/workflows/. The Essential row describes the recreated policy required for the 1.4.0 candidate; its remote authorization awaits the actual tag workflow. Historical 1.3.0 login/uploads used the preceding repository name. Package scope must cover all six existing IDs, and the policy must be active. Check GitHub environment protections/tag permissions and the profile variable before release. See [official Trusted Publishing guidance](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing).

## Future release procedure

Prepare is the default of publish-helper.bat / scripts/Publish-Helper.ps1. Publish requires the authorized source commit, a clean tree including untracked files, master, HEAD equal to fetched origin/master, a stable three-part VersionPrefix and no existing matching tag. The helper asks for the exact vVersionPrefix value before creating and pushing an annotated tag. CI checks that the tagged commit is contained in origin/master. Branch pushes, pull requests and workflow_dispatch build/verify only.

The tag-only workflow downloads and re-verifies its artifact before pushing in manifest order with --skip-duplicate. Do not replace existing tags or released versions. Essential v1.3.0, Flourish v1.1.0 and v1.1.1 retain their original commits; the two v1.1.0 attempts skipped publication and the corrected six-package version is 1.1.1. For a partial or failed release, inspect actual workflow/package state and obtain the effective authorization for the next version. Verify workflow success, public indexing and a fresh-cache public-source consumer as separate acceptance steps.

Published tags retain their original source commits. The 1.4.0 candidate requires a separately authorized commit and subsequent tag publication; this preparation does not authorize those actions. Follow AGENTS.md and the user's task-scoped authorization.

## User-operated acceptance

- Run the Blazor Gallery in two independent browser sessions and confirm language changes stay within their scope.
- Check UI language separately from number/date formatting, then refresh and verify the host culture cookie restores the selection.
- Run desktop Galleries and confirm translated bindings refresh on language changes.
- In a new application referencing only the public Blazor package, check generated keys and encoded translated output without a direct Generator reference.

No Computer Use acceptance was performed. Candidate consumers passed locally; public-only acceptance is completed for historical 1.3.0 and awaits future 1.4.0 publication. [Release verification](release-verification.md) retains current evidence and historical snapshots; existing append-only changes and bug reports remain unchanged. Historical username/publication statements describe their dated releases; they do not certify the recreated Essential policy.
