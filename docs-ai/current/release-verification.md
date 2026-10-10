# Culture release verification

## Current local 1.4.0 candidate verification on 2026-10-09

The approved language policy, eager resource modules, incremental generation and bounded selection cache are implemented. Release metadata targets `https://github.com/Evigila/Essential`, version **1.4.0**, and the same six existing package IDs. This is local preparation evidence; no commit, tag, push, Trusted Publishing login or public upload was performed.

The independent final `Test-Release.ps1 -ArtifactsPath ./artifacts/culture-1.4-final` completed successfully. **201 tests passed**, with no failures or skips: Core 89, Generator 82, Blazor 26 and WinUI markers 4. The source solutions and all five source-mode Galleries built with zero warnings/errors. Six packages passed identity/version/internal-graph, repository/project URL, current README, MIT license, required assets and public XML documentation checks. Evidence: `artifacts/culture-1.4-final.log`.

The user subsequently requested accuracy maintenance of only the existing module English/Simplified Chinese READMEs and authorized commit/tag/publication. Those two files now clarify .NET 10 requirements, all six packages, Blazor scopes, WinUI lifecycle, contexts, language policy, modules and measurement boundaries. Their 24 links map to existing repository targets, code fences balance and 14 XML examples parse. All six packages were repacked from the already verified assemblies and passed the current README/source-asset verifier again; no source behavior changed and functional tests were not redundantly rerun. Evidence: `artifacts/culture-1.4-readme-pack.log` and [the authorization record](1_changelogs/2026-10-09_215921_authorize-readme-and-culture-release.md).

The last Generator target refinement removes an empty output ledger when returning to the legacy single-file path. Its final independent Generator run passed 82/82 with zero warnings. The Generator was then repacked into the candidate set; the final verifier additionally matched every packaged buildTransitive asset against the current source SHA-256. All six packages passed. A new isolated module consumer built, ran, published and ran the published result with zero warnings/errors. Evidence: `artifacts/culture-1.4-final-modules.log` and `artifacts/package-consumers/modules-ba9b3d647636483599b625bca420d24c`. This follow-up supersedes the earlier module consumer for final target-asset evidence.

The isolated Blazor-only consumer at `artifacts/package-consumers/blazor-09a6b89c4c264c42926489a6592fb5ce` has one direct Blazor package reference and verifies transitive Core/Generator, generated keys, independent scopes, formatting and encoded rendering. The module-only consumer has one direct Core reference and verifies transitive Generator, root/feature keys, generated deployment manifest, denied-language selection and build/publish resource paths. Both use fresh caches and the local candidate feed.

All five Galleries also restored and built in **package mode**, `UseLocalCulture=false`, with a separate fresh cache and explicit source mapping: Culture IDs come from the candidate feed, other declared dependencies from NuGet.org. All five built with zero warnings/errors. Evidence: `artifacts/culture-1.4-gallery-packages.log`, `artifacts/culture-1.4-gallery-packages/obj/`, `artifacts/culture-1.4-gallery-cache/` and `artifacts/culture-1.4-gallery.NuGet.Config`. This closes adapter package-adoption coverage independently of source-mode builds.

The initial preparation exposed an inline-task compiler warning, which was fixed. A later run shared output files with concurrent focused tests and produced file-lock warnings; the final complete run used an independent directory and was clean. An initial supplemental Gallery restore misinterpreted a command-line source URI as a local path; the successful run uses the explicit NuGet.Config above. None of these earlier attempts are presented as warning-free final evidence.

[Performance evidence](culture-performance.md) retains both the complete matrix and the isolated repeat. Two-of-thirty language retention reduced completed catalog memory by 92.24%; measured successful raw/token lookups allocated zero bytes. Cold timing varies, and strict validation still makes substantial temporary allocations. No universal optimality claim or runtime UI acceptance is inferred.

The recreated Trusted Publishing policy must match owner `Evigila`, repository `Essential`, filename `build.yml`, environment `nuget`, the intended NuGet profile and all six package IDs. Repository/package identity and tag/workflow/environment/OIDC preflight are prepared; the actual remote policy can only be validated by the future authorized tag workflow's `NuGet/login` and uploads. Historical 1.3.0 success under the old repository name does not validate this policy. Public indexing and fresh-cache public-only consumption follow publication as separate checks.

Manual acceptance remains user-operated without Computer Use. Use the [resource/module and UI checklist](culture-resource-usage.md#manual-acceptance) and [release guide](nuget-release-integration.md#future-release-procedure). The audited [API inventory](culture-public-api.md) now covers 19 handwritten public types and the generated/build contracts. Documentation inventories are synchronized with the candidate; the completed full-audit timestamp is preserved.

## Historical coordinated release state on 2026-10-06

Essential.Culture 1.3.0 is published, publicly indexed and verified by fresh-cache public-only consumption. Source commit [983c636bdba7e1a7b74cee33b5c682c9bf06364e](https://github.com/Evigila/Essential.Culture/commit/983c636bdba7e1a7b74cee33b5c682c9bf06364e) is tagged v1.3.0. [Run 37537838178](https://github.com/Evigila/Essential.Culture/actions/runs/37537838178) succeeded, including Trusted Publishing login and six Created uploads. All six public flat-container indexes returned HTTP 200 and contain 1.3.0.

The public-source-only consumer at artifacts/public-consumers/blazor-1e4f292745bf4a2fbe68954d77b4132a restored into a fresh cache, built with warnings as errors and ran successfully. Its only direct package is Arkheide.Essential.Culture.Blazor; Core and Generator restore transitively. Generated keys, independent scoped languages, formatting culture and encoded rendering passed.

Flourish 1.1.1 completed its GitHub release workflow and uploaded all six packages. Source commit [05ebb4d282eea7130fc9326ef2d15bf43593d5aa](https://github.com/Evigila/Flourish/commit/05ebb4d282eea7130fc9326ef2d15bf43593d5aa) is tagged v1.1.1. [Run 37540483235](https://github.com/Evigila/Flourish/actions/runs/37540483235) completed successfully: [build job 112531999414](https://github.com/Evigila/Flourish/actions/runs/37540483235/job/112531999414) passed, including the corrected CSS fixture; [publish job 112533073982](https://github.com/Evigila/Flourish/actions/runs/37540483235/job/112533073982) completed Trusted Publishing login and uploaded the six packages in manifest order with Created responses from 22:28:55 to 22:29:01 UTC on 2026-10-06.

All six Flourish 1.1.1 public flat-container indexes now return HTTP 200 and contain 1.1.1. Fresh-cache, public-only verification completed with exit code 0 across FrameworkOnly, MetaNative, MetaDesign and MetaCulture: all 129 checks passed, covering DI activation, SSR, CSS/woff2/JavaScript assets, three languages and generated keys. Evidence: Flourish/artifacts/public-culture-release-1.1.1.log and Flourish/artifacts/package-consumers/027abe99e60f46dbaa1d15a75b670ff6. Its NuGet.Config contains one public NuGet.org source with a wildcard mapping. Every .nupkg.metadata for the six Flourish 1.1.1 packages and Essential.Blazor/Core/Generator 1.3.0 identifies https://api.nuget.org/v3/index.json; no local feed or source project reference is used. Essential's independent public-only consumer also passed. Publication, public indexing and public consumption are complete for both releases.

Essential's six-package release and public consumer are complete. Flourish's successful build/login/Created uploads supersede the preceding pending correction/publication statements; its public indexing and four-mode public-only consumer are now complete. Both repositories use vars.NUGET_USER || secrets.NUGET_USER in environment nuget, with no local long-lived API key. The corrected Flourish CSS fixture passed the final build; v1.1.0 tags were preserved.

Manual acceptance remains user-operated: independent Blazor sessions, UI versus formatting culture, cookie reload persistence and desktop Gallery language refresh. No Computer Use acceptance was performed. The retained evidence below is historical; earlier statements awaiting upload, fixing 1.1.1, indexing or public consumption are superseded by this completed-release section.

## Final public verification and documentation boundary

The public-only command & ./build/Test-BlazorPackageConsumers.ps1 -PublicSource -Version 1.1.1 completed with exit code 0. Its single-source NuGet.Config maps all packages to NuGet.org and rejects local package-directory parameters; no project reference or sibling feed participates. All six Flourish package metadata entries and the three transitive Essential metadata entries identify public https://api.nuget.org/v3/index.json.

The initial public attempt failed NU1100 because duplicate names for the same NuGet.org URI collapsed to a restrictive Flourish.* mapping, excluding Microsoft.AspNetCore.App.Internal.Assets 10.0.11. Failed fixture Flourish/artifacts/package-consumers/d1afce7bb6784b64be6730553c6e64a3 retains its original config/assets/diagnostics. The explicit PublicSource verification mode fixes that fixture configuration without altering default local/CI modes or any published nupkg. Final evidence is Flourish/artifacts/public-culture-release-1.1.1.log and Flourish/artifacts/package-consumers/027abe99e60f46dbaa1d15a75b670ff6.

The published tags retain their original source commits. Final technical documentation and the public-verification helper correction will be committed separately under the user's existing commit/publication authorization; they do not create a new release tag or republish packages. Future commits and publication follow AGENTS.md and the user's task-scoped authorization.

## Earlier completed release verification: Essential.Culture 1.3.0 on 2026-10-06

All six Essential.Culture 1.3.0 packages are published and publicly indexed. Source commit [983c636bdba7e1a7b74cee33b5c682c9bf06364e](https://github.com/Evigila/Essential.Culture/commit/983c636bdba7e1a7b74cee33b5c682c9bf06364e) is tagged v1.3.0. [Workflow run 37537838178](https://github.com/Evigila/Essential.Culture/actions/runs/37537838178) succeeded: Trusted Publishing login passed and all six package uploads returned Created. The six NuGet flat-container indexes returned HTTP 200 and contain 1.3.0:

| Package | Public version index |
|---|---|
| Arkheide.Essential.Culture.Generator | [Generator](https://api.nuget.org/v3-flatcontainer/arkheide.essential.culture.generator/index.json) |
| Arkheide.Essential.Culture | [Core](https://api.nuget.org/v3-flatcontainer/arkheide.essential.culture/index.json) |
| Arkheide.Essential.Culture.Wpf | [Wpf](https://api.nuget.org/v3-flatcontainer/arkheide.essential.culture.wpf/index.json) |
| Arkheide.Essential.Culture.Avalonia | [Avalonia](https://api.nuget.org/v3-flatcontainer/arkheide.essential.culture.avalonia/index.json) |
| Arkheide.Essential.Culture.WinUI | [WinUI](https://api.nuget.org/v3-flatcontainer/arkheide.essential.culture.winui/index.json) |
| Arkheide.Essential.Culture.Blazor | [Blazor](https://api.nuget.org/v3-flatcontainer/arkheide.essential.culture.blazor/index.json) |

A fresh-cache consumer restored only from public NuGet sources at artifacts/public-consumers/blazor-1e4f292745bf4a2fbe68954d77b4132a. Restore, build with warnings as errors and execution all passed. Its sole direct package is Arkheide.Essential.Culture.Blazor; Core and Generator restore transitively. Generated keys, independent scoped languages, explicit formatting culture and encoded rendering passed. This is public-package consumption evidence, distinct from the earlier local candidate feed.

Release preparation passed 106 automated checks (Core 61, Generator 20, Blazor 21, WinUI 4); all five Galleries compiled with zero warnings/errors. Runtime publication verifies the current Essential policy's authorization for this exact six-package release, including the new Blazor ID. It does not establish permission for future unrelated package IDs.

Flourish has not been published. Both v1.1.0 workflow attempts failed in CSS verification because its fixture lacked the default package cache; publish was skipped in both. A later reproduction also encountered NU1301. The coordinating task is fixing the release path and advancing Flourish to 1.1.1. The existing v1.1.0 tag will not be overwritten. Essential's successful release must not be reported as a Flourish release.

Manual acceptance remains user-operated without Computer Use: exercise independent Blazor sessions, culture versus formatting culture, cookie reload persistence and desktop Gallery language refresh. The public consumer's automated restore/build/execution passed; physical browser and desktop interaction are not claimed.

## Earlier preparation and configuration evidence

The retained snapshots below preceded publication. Their pending account/commit/public-index boundaries describe those earlier runs, not the completed Essential 1.3.0 release above. Flourish's local preparation results are not evidence of a public Flourish release.

## Previous preparation: 2026-10-06 repository metadata and dependent release

Essential's preceding full preparation passed 106 tests: Core 61, Generator 20, Blazor 21 and WinUI 4. All five Galleries compiled with zero warnings/errors and all six fresh 1.3.0 packages passed identity/dependency/asset verification. The existing isolated Blazor-only consumer verifies Core/Generator transitively with no direct Generator reference.

This round subsequently changed RepositoryUrl/PackageProjectUrl to Evigila/Essential.Culture, repacked all six libraries and passed Verify-PackageSet. All six nuspec files were checked and use the current Essential.Culture URL. These metadata-only changes did not require rerunning functional tests; the 106-test result belongs to the preceding complete preparation.

Dependent Flourish's complete staged preparation completed with exit code 0 and zero build warnings/errors: Core 367, Blazor 373/373, bridge 12, Gallery 7,234 including 124 real-event language-switch/state-retention checks, Node 66, CSS bundle 21 and SDK integration 194, catalog 21,217 and four isolated consumers totaling 129 checks. Exactly six fresh Flourish Core/Blazor 1.1.0 packages passed verification; WPF was excluded. Evidence is Flourish/artifacts/culture-final-release.log and Flourish/artifacts/package-consumers/01e52f8b29ce48f89c139dfc26a2bfbb. Its initial consumer HTTP 500 was corrected by adding [FromServices] to a Minimal API fixture; no production behavior change was needed.

Flourish Gallery's bridge package migration is complete: its sole bridge PackageReference uses VersionPrefix, and Generator is obtained transitively. Preparation now packs/verifies the targeted six-library umbrella graph before restoring the full Blazor solution through temporary candidate-source mapping and a fresh cache, asserting Gallery package adoption, running build/tests/checks and checking four package consumers. EssentialPackageDirectory explicitly supplies local Essential candidates; no implicit sibling feed or source fallback is used.

The user supplied NuGet profile Evigila and is configuring Flourish's Trusted Publishing policy/secret. Account-policy permissions, coverage for the new Blazor ID and configuration completion remain unconfirmed. A profile name or historical 1.2.0 Trusted Publishing success does not confirm present authorization. Local preparation did not create a new commit, tag, push or public NuGet release. The user has authorized Trusted Publishing execution; a new commit still awaits the user's answer under AGENTS.md. Public indexing and a clean public-source consumer remain release acceptance steps.

Manual acceptance remains user-operated without Computer Use: check two independent Blazor browser sessions, UI versus formatting culture, cookie reload persistence/invalid culture handling, desktop demo language refresh and generated keys. In Flourish, use the original Gallery start.bat and check icons/styles, same-page H1/H2/body/sample/validation translation and input/selection retention in all three languages. Public-source acceptance follows publication and is not established by local candidates.

## Historical verification snapshots from 2026-10-05 and earlier 2026-10-06 audit

The following results and configuration observations retain their dated scope. Earlier current/latest wording, 89-test counts, package scopes and pending requests describe those runs; the latest section above and active release guide supersede them.

The current preparation task reran the full publish-helper.bat -Mode Prepare for Essential.Culture 1.3.0 on the RC_Auditoria checkout and completed with exit code 0, including the new isolated Blazor-only NuGet consumer. This document distinguishes the current local run from earlier coordinating verification and external configuration observations. No public NuGet release was performed.

## Previous verification

- All 89 automated tests passed: Core 53, Generator 11, Blazor 21 and WinUI 4, with no skipped tests.
- All five demos compiled; the solution and demo builds reported zero warnings and errors.
- All six 1.3.0 packages passed identity, version, dependency and required-asset inspection and are available at C:/Users/RC_Auditoria/source/Repos/Essential/artifacts/packages.
- The Blazor-only consumer restored solely from that local package directory into a new isolated package cache. Its only direct PackageReference is Arkheide.Essential.Culture.Blazor; the restore graph contains Core and Generator 1.3.0 transitively.
- Generated Key members compiled without a direct Generator reference. The packaged consumer ran checks for Core catalog lookup, independent scoped languages, explicit formatting culture and encoded LocalizedText output.
- The final full Prepare included that consumer gate and completed successfully. Its retained output is artifacts/package-consumers/blazor-d9ebe1b4f154492b9bb39e99b55b0069; subsequent runs use a new directory and cache.
- Runtime/platform APIs and version 1.3.0 were unchanged. The edits add the package-consumer fixture, its verification script, the Prepare gate and this task's documentation.

The consumer fixture lives under tests/package-consumers/Tests.Essential.Culture.Blazor.PackageConsumer. Run scripts/Verify-BlazorPackageConsumer.ps1 to repeat only the packaged consumption check after preparing the package set. It builds and runs a local consumer; it is not an inspection-only script or a public-source availability test.

## Project naming verification on 2026-10-05

Four regression projects now use Tests.<target>; five runnable examples use Gallery.<target>, and the isolated consumer uses Tests.Essential.Culture.Blazor.PackageConsumer. The Gallery solution is demo/Gallery.Essential.Culture.slnx. Renamed projects explicitly retain their previous assembly identities and namespaces so XAML assembly references, internal test access and generator configuration do not change. The six library projects and six 1.3.0 NuGet identities are unchanged.

The full scripts/Test-Release.ps1 rerun completed with exit code 0: 89 tests passed, all five renamed Galleries compiled with warnings as errors, all six packages passed inspection, and the independent Blazor-only consumer passed at artifacts/package-consumers/blazor-206e03a49b9c41e680bbbb21f1de30a4. Static path validation covered 16 maintained project files, 22 ProjectReference paths and three solutions. No public release, Git commit, tag or push occurred.

## Historical coordinating verification

- All 89 automated tests passed.
- All five demo projects compiled.
- All six configured 1.3.0 packages passed package identity, version, dependency and required-asset verification.
- Essential source was unchanged after that full preparation.
- The dependent Flourish final Prepare completed with exit code 0 and verified its eight 1.1.0 packages against packaged Culture dependencies.
- Local dotnet publish consumers served their tested static resources successfully: 18 HTTP 200 responses for the independent Framework-only consumer and 23 for Colligere. These application output checks are not NuGet publication.

Use [NuGet release and integration](nuget-release-integration.md) for the six package IDs, module version ownership, helper parameters and user-confirmed release order.

## Previously observed external configuration

Read-only GitHub inspection confirmed Evigila/Arkheide.Essential.Culture's existing nuget environment and NUGET_USER secret name. The environment has protection_rules=[] and branch_policy=null. The secret value was not retrieved. The NuGet.org account-side trust policy and package scope remain unverified.

The coordinating task subsequently created Evigila/Flourish's nuget environment with the same absence of protection rules/branch policy. Flourish's NUGET_USER remains unconfigured while the user supplies its NuGet profile username. No API key was requested.

The coordinating task queried all 14 release package IDs: all six Essential 1.3.0 and eight Flourish 1.1.0 target versions remain unpublished. Five existing Essential platform packages have latest version 1.2.0; Culture.Blazor and all eight Flourish IDs returned 404. Publish all six Essential packages and confirm their public availability before the Flourish release. Neither an existing environment nor a local package set proves that publishing configuration is complete.

## Approval review and remaining boundary

Automatic approval review rejected a Publish-mode negative test because the helper can fetch, create tags and push. The coordinating task substituted a read-only AST guard inspection rather than executing that mode.

No commit, tag, push or NuGet publication occurred. A future Publish remains separately authorized and subject to the script's clean master, origin/master equality and exact v1.3.0 confirmation. This verification did not modify human README/documents or historical records.

The current preparation task did not rerun external environment or account-policy checks. It created no commit, tag, push or publication. The new uncommitted verification/documentation files must be reviewed and committed before Publish's clean-tree requirement can pass.

## Manual regression checks

- Run the Blazor demo in two independent browser sessions, switch one language and confirm the other session retains its selection.
- Check UI language and formatting culture independently, including the Chinese UI with Brazilian number/date formats.
- Submit the host's culture form, refresh and confirm its cookie restores the selected language; reject invalid cultures and invalid antiforgery submissions.
- Run the desktop demos and verify language switching still refreshes their bound text.

No Computer Use testing was performed. The checks above remain user-operated UI acceptance checks.

## 2026-10-06 Trusted Publishing correction and current evidence

This section supersedes treating the earlier secret/profile request or local API-key note as the current release boundary. The user has chosen agent-executed Trusted Publishing. Both repositories already use NuGet/login@v1 with NUGET_USER, id-token: write and the nuget environment. NUGET_USER is a NuGet.org profile username; no local long-lived API key, extra nuget.exe or GitHub CLI is required. New source commits still require the user's answer under AGENTS.md.

Read-only checks identified the current repositories as Evigila/Essential.Culture (ID 1327295083) and Evigila/Flourish (ID 1246107902), using build.yml and nuget policy fields. Both public environment API responses confirmed existence, zero protection rules and null branch policy. Secret presence/value and the current NuGet account policy/package permissions remain unverified. In particular, historical success must not be treated as authorization for creating the new Blazor package ID or publishing the new target versions.

The [Essential v1.2.0 run 33030395528](https://github.com/Evigila/Essential.Culture/actions/runs/33030395528) completed Trusted Publishing login and push steps successfully. The [Essential master run 37291575812](https://github.com/Evigila/Essential.Culture/actions/runs/37291575812) succeeded without publishing. The [Flourish run 37291596951](https://github.com/Evigila/Flourish/actions/runs/37291596951) failed during its older full-solution restore because Essential.Blazor was absent and Wpf 1.3.0 was not published; publish was skipped before OIDC login. This is not a Trusted Publishing authentication failure or evidence of the result of today's focused release preparation.

The active [release guide](nuget-release-integration.md) now records exact package scope/order, the umbrella's automatic Culture bridge, explicit EssentialPackageDirectory instead of an implicit sibling feed, isolated ArtifactsPath and the pending Gallery source-bridge consumption migration. Earlier preparation totals and external observations retain their dated scope. This documentation/audit task performed no new commit, release tag, push or NuGet publication; current preparation and public-index checks must be reported separately.
