# Culture NuGet release and integration

Essential.Culture 1.3.0 is the current six-package release target. The user has selected agent-executed NuGet Trusted Publishing. This documentation task did not create a commit, tag or push and did not publish packages. Historical preparation results and current public release evidence are distinct.

## Module and package boundaries

Essential.slnx contains the Culture module and tests. The focused module solution is src/Essential.Culture/Essential.Culture.slnx, with examples in demo/Gallery.Essential.Culture.slnx. VersionPrefix=1.3.0 is owned by src/Essential.Culture/Directory.Build.props; scripts/ReleaseSettings.psd1 publishes in this order:

1. Arkheide.Essential.Culture.Generator
2. Arkheide.Essential.Culture
3. Arkheide.Essential.Culture.Wpf
4. Arkheide.Essential.Culture.Avalonia
5. Arkheide.Essential.Culture.WinUI
6. Arkheide.Essential.Culture.Blazor

Generator has no runtime library dependency. Core owns immutable catalogs, contexts and the existing desktop facade. Wpf, Avalonia, WinUI and Blazor depend on Core. Essential does not reference Flourish; Flourish consumes the packaged Culture libraries through its bridge. NuGet IDs are unchanged by the GitHub repository's current name, Evigila/Essential.Culture.

## Blazor consumer contract

AddCultureBlazor configures immutable catalogs and a request/circuit-scoped ILocalizationService. The service exposes Culture, FormatCulture, AvailableCultures, Changed, SetCulture, Parse/ParseFrom, TryParse/TryParseFrom and Contains/ContainsFrom. Catalog IDs separate host and library text. Formatted TryParse overloads let bridges distinguish a successful lookup from a fallback.

LocalizationBuilder provides AddCatalog, SetDefaultCatalog, SetDefaultCulture, AddSupportedCultures and InitializeWith. Hosts own negotiation, cookies and initial culture selection. LocalizedComponentBase refreshes on Changed and removes its subscription on disposal. LocalizedText encodes output from Key, CatalogId, Arguments and Fallback.

SSR and Interactive Server are the verified targets. Standalone browser WebAssembly is not verified. Web hosts use the scoped service rather than the process-wide desktop Localizer facade. Desktop behavior remains separate.

## Local preparation

The existing batch entry forwards to scripts/Publish-Helper.ps1; Prepare is the default. No long-lived local NuGet API key, extra nuget.exe or GitHub CLI is required by the Trusted Publishing path.

```powershell
Set-Location C:\Users\RC_Auditoria\source\Repos\Essential
.\publish-helper.bat -Mode Prepare
# Or select an explicit SDK artifacts directory:
.\scripts\Test-Release.ps1 -ArtifactsPath .\artifacts\release-build
```

Test-Release.ps1 restores/builds in Release with warnings as errors, runs the tests, builds the five demos with -p:UseLocalCulture=true, packs all six libraries and runs Verify-PackageSet.ps1 plus the isolated Blazor-only package consumer. ArtifactsPath isolates output and intermediate files together through the SDK artifacts layout. Preparation replaces local .nupkg/.snupkg files in artifacts/packages; it does not publish or create commits/tags.

Demos otherwise default to PackageReference. A source-mode demo build does not prove packaged consumption. The Blazor-only fixture directly references only Arkheide.Essential.Culture.Blazor, restores Core and Generator transitively into a fresh isolated cache and checks generated keys and scoped/encoded rendering. Public-source restore after release is a separate check.

-SkipBuild on the helper and -VerifyOnly on Test-Release.ps1 inspect existing packages; they do not prove those packages match current source and do not waive Publish's Git/tag checks. Verify-PackageSet.ps1 accepts an explicit PackageDirectory and Version.

## Trusted Publishing configuration and evidence

Both Essential and Flourish already use .github/workflows/build.yml with a nuget environment, contents: read and id-token: write. NuGet/login@v1 receives secrets.NUGET_USER and returns a short-lived NUGET_API_KEY for dotnet nuget push. The agent does not need a long-lived API key on this computer. NUGET_USER is the NuGet.org profile username, not an email or API key. The user supplied Evigila on 2026-10-06 and is configuring Flourish's policy/secret; completion of those settings and the current account policy remains unconfirmed.

The current Essential policy fields are repository owner Evigila, repository Essential.Culture (GitHub ID 1327295083), workflow build.yml and environment nuget. Flourish uses Evigila/Flourish (ID 1246107902), also build.yml and nuget. The workflow field does not include .github/workflows/. Use the current remote identity instead of the former Arkheide.Essential.Culture repository name or local folder name. See [official NuGet Trusted Publishing guidance](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing).

Confirm that the NuGet.org policy permits creating package IDs and publishing versions under the intended owner for all six exact IDs above. In particular, a successful old five-family release does not establish authorization for the new Arkheide.Essential.Culture.Blazor package. The authenticated account's current policy, package scope and NUGET_USER presence/value were not inspected by this audit; a historical secret-name observation is insufficient to prove current configuration.

| Evidence checked on 2026-10-06 | Result |
|---|---|
| [v1.2.0 run 33030395528](https://github.com/Evigila/Essential.Culture/actions/runs/33030395528) | Trusted Publishing login and package-push steps succeeded for that release. |
| [Master run 37291575812](https://github.com/Evigila/Essential.Culture/actions/runs/37291575812) | Build succeeded without publication. |
| [Essential nuget environment](https://api.github.com/repos/Evigila/Essential.Culture/environments/nuget) | Exists; zero protection rules, null branch policy. |
| [Flourish nuget environment](https://api.github.com/repos/Evigila/Flourish/environments/nuget) | Exists; zero protection rules, null branch policy. |
| [Flourish run 37291596951](https://github.com/Evigila/Flourish/actions/runs/37291596951) | The older full-solution restore failed on absent Essential.Blazor and unpublished Wpf 1.3.0; publish was skipped before OIDC login. |

Environment existence is not proof of NuGet account authorization. Neither current environment has an API-observed approval gate. The successful 1.2.0 publication proves the historical Trusted Publishing path, not current six-package policy coverage or a new 1.3.0 release.

## Dependent Flourish preparation

Publish all six Essential 1.3.0 packages and verify their public indexing before publishing Flourish's six Core/Blazor 1.1.0 packages. Flourish's umbrella now automatically installs its Culture bridge; Framework-only consumers can omit Design and Culture. Flourish WPF is outside this release. Its order is Core, Abstract, Culture bridge, Framework, Design and umbrella.

Flourish has removed its implicit sibling Essential/artifacts/packages restore feed. For local preparation before public availability, explicitly pass -EssentialPackageDirectory to Flourish scripts/Test-Release.ps1; the directory is a NuGet feed and never selects source ProjectReference dependencies. Gallery now uses its single Flourish Culture bridge PackageReference at VersionPrefix, without a source bridge or direct Generator reference. Flourish first restores/builds/packs/verifies the targeted umbrella's six libraries, then restores the complete Blazor solution against a temporary candidate-source mapping and fresh cache, asserts Gallery package adoption and runs build/tests/checks before its four isolated package consumers. This staged preparation bootstraps the first bridge release before public indexing. See the coordinated [Flourish release guide](../../../Flourish/docs-ai/current/nuget-release-integration.md) for the exact package scope, local-feed and consumer boundaries.

## Local preparation verified on 2026-10-06

The preceding complete Essential preparation passed 106 automated tests: Core 61, Generator 20, Blazor 21 and WinUI 4. All five Galleries compiled with zero warnings/errors, and all six fresh 1.3.0 packages passed verification. This round then corrected RepositoryUrl/PackageProjectUrl to the current Evigila/Essential.Culture identity, repacked all six libraries and passed Verify-PackageSet again. Inspection of all six nuspec files confirmed the current repository URL. The metadata-only repack did not rerun functional tests and does not change their recorded scope.

Dependent Flourish's staged preparation also succeeded: zero build warnings/errors, Core 367, Blazor 373/373, bridge 12, Gallery 7,234 (including 124 real-event language-switch/retention checks), Node 66, CSS 21/194, catalog 21,217 and four isolated consumers totaling 129 checks. Its exact six fresh Core/Blazor 1.1.0 candidates were verified, with WPF excluded. Retained Flourish evidence is artifacts/culture-final-release.log and artifacts/package-consumers/01e52f8b29ce48f89c139dfc26a2bfbb. Local preparation is complete; public indexing, account-policy completion and the user's answer before a new commit remain separate boundaries. See [local release verification](release-verification.md).

## Authorized publication

The user has authorized agent-executed publication through Trusted Publishing. AGENTS.md still requires the user's answer before a new source commit. Review and commit the intended release, synchronize master, complete package preparation and verify the account policy before the helper runs. This audit did not commit, tag, push or publish.

Publish requires a clean tree including untracked files, the master branch, HEAD equal to fetched origin/master, a stable three-part VersionPrefix and no existing matching version tag. It asks for the exact version tag before creating and pushing an annotated tag. The CI tag check verifies that the tagged commit is contained in origin/master. Ordinary branch pushes, pull requests and workflow_dispatch build and verify only.

```powershell
.\publish-helper.bat -Mode Publish
# Exact tag confirmation: v1.3.0
```

The tag-only job downloads and re-verifies the build artifact, logs in through Trusted Publishing and pushes in manifest order with --skip-duplicate. Do not replace an existing release tag or package version. Confirm workflow success, all six public 1.3.0 versions and a clean public-source consumer before releasing dependent Flourish packages.

## Historical observations from 2026-10-05

The coordinating task saw the then-named Evigila/Arkheide.Essential.Culture nuget environment and NUGET_USER secret name without reading its value or account policy. It subsequently created Flourish's unprotected nuget environment; the Flourish username request was pending then. These observations do not determine today's secret or policy state.

The then-current queries found all six target Essential 1.3.0 versions unpublished; five existing families were indexed through 1.2.0 while Blazor was absent. Earlier Flourish eight-package checks remain historical; the current scope is six Core/Blazor packages. The previous local preparation reported 89 tests and five demo builds. None of these historical results establishes current package artifacts or a new public release.

The 2026-10-06 historical prerequisite record suggesting a required local API key is superseded by the user's Trusted Publishing correction and the verified workflow. Existing append-only change records remain unchanged.

## Source commit authorization on 2026-10-06

The user explicitly authorized one release-preparation commit in Essential and one in Flourish, including the verified pending localization, control, project-naming, package-consumption and release-workflow changes. This resolves the commit-answer requirement under AGENTS.md. NuGet account-policy and NUGET_USER setup completion remains to be confirmed before version-tag publication. This authorization does not claim that packages are publicly available.

## Confirmed Trusted Publishing profile on 2026-10-06

The user reported completing the corresponding NuGet Trusted Publishing configuration and confirmed the profile username Evigila. The login action now receives that public profile name directly as user: Evigila, following the official action contract. A GitHub variable called nuget is not the same as secrets.NUGET_USER. The workflow therefore no longer requires a NUGET_USER secret or variable; no long-lived API key is introduced. This supersedes the earlier pending username/secret setup statements. The policy's actual authorization and new-package scope still require successful OIDC login and package publication as runtime evidence. The user has also authorized both release-preparation commits and agent-executed publication.

## Clarified environment username on 2026-10-06

The user clarified that nuget names the existing GitHub environment and that NUGET_USER has been configured. Login now reads the named GitHub configuration as vars.NUGET_USER || secrets.NUGET_USER: an Actions Variable is used when present, otherwise the existing Secret is used. The username is not hard-coded. This supersedes the preceding direct-profile input decision while preserving existing Essential secret storage and the user's Flourish variable storage. The user has reported Trusted Publishing setup complete; workflow execution will verify actual login and package-scope authorization. The two agent-created commits are still local and will be updated before their first push.
