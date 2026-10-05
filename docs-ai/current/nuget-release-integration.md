# Culture NuGet release and integration

Essential.Culture 1.3.0 is the current release target. Its six packages are prepared together from the Essential root, while the version remains scoped to src/Essential.Culture/Directory.Build.props. Local preparation and package inspection have passed. No public NuGet publication or release-tag push was performed by this task.

## Module and package boundaries

The root Essential.slnx contains the Culture module and its tests. The focused module solution is src/Essential.Culture/Essential.Culture.slnx. Demo projects belong to demo/Essential.Culture.Demo.slnx.

scripts/ReleaseSettings.psd1 reads the module's VersionPrefix and lists this dependency order:

1. Arkheide.Essential.Culture.Generator
2. Arkheide.Essential.Culture
3. Arkheide.Essential.Culture.Wpf
4. Arkheide.Essential.Culture.Avalonia
5. Arkheide.Essential.Culture.WinUI
6. Arkheide.Essential.Culture.Blazor

All six use version 1.3.0. Generator has no runtime library dependency; Core exposes the existing catalog/context and desktop facade. Wpf, Avalonia, WinUI and Blazor depend on Core. Essential does not reference Flourish. Flourish's optional integrations consume the packaged Culture libraries.

## Blazor consumer contract

AddCultureBlazor configures immutable catalogs and registers ILocalizationService per request or circuit. Its public service exposes Culture, FormatCulture, AvailableCultures, Changed, SetCulture, Parse/ParseFrom, TryParse/TryParseFrom and Contains/ContainsFrom. Explicit catalog IDs keep host and library wording separate; formatted TryParse overloads support a bridge without guessing whether fallback text is a successful lookup.

LocalizationBuilder supports AddCatalog, SetDefaultCatalog, SetDefaultCulture, AddSupportedCultures and InitializeWith. Hosts own negotiation, cookies and initial session selection. LocalizedComponentBase dispatches refresh on Changed and removes its subscription when disposed. LocalizedText renders encoded text with Key, CatalogId, Arguments and Fallback.

These APIs target SSR and Interactive Server. Standalone browser WebAssembly use has not been validated. Web hosts use the scoped service rather than the process-wide desktop Localizer facade. Desktop behavior remains covered by its existing tests.

## Preparation and publication

The local batch entry forwards to scripts/Publish-Helper.ps1:

```powershell
Set-Location C:\Users\Evigila\source\repos\Essential
.\publish-helper.bat -Mode Prepare
```

Prepare is the default. Test-Release.ps1 restores/builds the root in Release with warnings as errors, runs the tests, builds the five demos with -p:UseLocalCulture=true, packs the six libraries and calls Verify-PackageSet.ps1. It replaces local artifacts/packages .nupkg/.snupkg files; it does not commit or publish. Demo source mode verifies the modified source, while package inspection verifies release dependency metadata and generator assets. Demo projects otherwise default to PackageReference.

Publish runs the same preparation. -SkipBuild selects verification of existing packages instead of a fresh build; it does not bypass Publish's clean master, fetched origin/master equality, version or confirmation checks. Direct Test-Release.ps1 -VerifyOnly and Verify-PackageSet.ps1 -PackageDirectory <path> -Version <version> are inspection-only options.

After reviewing, committing and pushing the intended release, the user may authorize:

```powershell
.\publish-helper.bat -Mode Publish
# At the confirmation prompt, type: v1.3.0
```

The helper requires a clean working tree including untracked files, master, HEAD equal to origin/master and no existing v1.3.0 tag. It asks for the exact tag before creating and pushing an annotated tag. A normal branch push, pull request or workflow_dispatch verifies packages without publishing. A matching tag push enables the NuGet job after build verification.

GitHub .github/workflows/build.yml uses the nuget environment and NUGET_USER secret with NuGet/login@v1. The current remote is Evigila/Arkheide.Essential.Culture; the checkout's Essential folder name does not rename that remote. The user configures its NuGet Trusted Publishing policy and environment approvals. The username and policy fields follow [official NuGet guidance](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing).

For the full two-repository policy field table and publication sequence, use Flourish/docs-ai/current/nuget-release-integration.md. Publish and confirm all six Essential 1.3.0 packages first, then prepare/publish Flourish's eight 1.1.0 packages. Flourish restores packaged Culture dependencies; sibling Essential/artifacts/packages is a local NuGet feed, not a source reference switch. The former Flourish EssentialCultureRoot/UseLocalEssentialCulture mechanism is retired.

## Configuration and publication status on 2026-10-05

The coordinating task's read-only GitHub inspection found this repository's nuget environment and NUGET_USER secret name. Its protection_rules=[] and branch_policy=null mean no environment protection or branch policy is currently configured. The secret value and NuGet.org account-side Trusted Publishing policy were not inspected. Confirm the profile and policy owner/scope before release.

The coordinating task queried all 14 package IDs from the two ReleaseSettings files. All six Essential 1.3.0 and eight Flourish 1.1.0 target versions remain unpublished. The five existing Essential platform packages have latest version 1.2.0; Culture.Blazor and all eight Flourish IDs returned 404. This is a complete target-set check, not a sample. Local verification does not establish public availability.

The coordinating task subsequently created the dependent Flourish repository's nuget environment, also without protection rules or branch policy. Flourish's NUGET_USER is still unconfigured and its user profile-name request is pending. Complete that configuration and verify both NuGet.org policies; confirm all six Essential target packages before the Flourish 1.1.0 release. See [final local verification](release-verification.md).

## Acceptance

- Verify all six exact package IDs/versions and internal dependencies.
- Verify generator analyzer and buildTransitive assets.
- Run the five demos and existing scoped-localization tests.
- Restore a clean package consumer without -p:UseLocalCulture=true.
- Confirm the NuGet environment/policy and all six publicly indexed package versions before a dependent Flourish release.
- Report public release only after workflow completion and NuGet.org visibility; local artifacts alone do not establish publication.
