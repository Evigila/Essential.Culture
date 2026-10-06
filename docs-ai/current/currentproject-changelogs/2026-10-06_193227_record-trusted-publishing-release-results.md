# Record coordinated Trusted Publishing release results

Recorded on 2026-10-06T19:32:27-03:00 (America/Sao_Paulo). Finalized on 2026-10-06T19:50:06-03:00 after public indexing and consumption completed, before this record's documentation commit.

## Verified results

Essential.Culture 1.3.0 is published, publicly indexed and verified by fresh-cache public-only consumption. Source commit [983c636bdba7e1a7b74cee33b5c682c9bf06364e](https://github.com/Evigila/Essential.Culture/commit/983c636bdba7e1a7b74cee33b5c682c9bf06364e) is tagged v1.3.0. [Run 37537838178](https://github.com/Evigila/Essential.Culture/actions/runs/37537838178) succeeded, including Trusted Publishing login and six Created uploads. All six public flat-container indexes returned HTTP 200 and contain 1.3.0.

The public-source-only consumer at artifacts/public-consumers/blazor-1e4f292745bf4a2fbe68954d77b4132a restored into a fresh cache, built with warnings as errors and ran successfully. Its only direct package is Arkheide.Essential.Culture.Blazor; Core and Generator restore transitively. Generated keys, independent scoped languages, formatting culture and encoded rendering passed.

Flourish 1.1.1 completed its GitHub release workflow and uploaded all six packages. Source commit [05ebb4d282eea7130fc9326ef2d15bf43593d5aa](https://github.com/Evigila/Flourish/commit/05ebb4d282eea7130fc9326ef2d15bf43593d5aa) is tagged v1.1.1. [Run 37540483235](https://github.com/Evigila/Flourish/actions/runs/37540483235) completed successfully: [build job 112531999414](https://github.com/Evigila/Flourish/actions/runs/37540483235/job/112531999414) passed, including the corrected CSS fixture; [publish job 112533073982](https://github.com/Evigila/Flourish/actions/runs/37540483235/job/112533073982) completed Trusted Publishing login and uploaded the six packages in manifest order with Created responses from 22:28:55 to 22:29:01 UTC on 2026-10-06.

All six Flourish 1.1.1 public flat-container indexes now return HTTP 200 and contain 1.1.1. Fresh-cache, public-only verification completed with exit code 0 across FrameworkOnly, MetaNative, MetaDesign and MetaCulture: all 129 checks passed, covering DI activation, SSR, CSS/woff2/JavaScript assets, three languages and generated keys. Evidence: Flourish/artifacts/public-culture-release-1.1.1.log and Flourish/artifacts/package-consumers/027abe99e60f46dbaa1d15a75b670ff6. Its NuGet.Config contains one public NuGet.org source with a wildcard mapping. Every .nupkg.metadata for the six Flourish 1.1.1 packages and Essential.Blazor/Core/Generator 1.3.0 identifies https://api.nuget.org/v3/index.json; no local feed or source project reference is used. Essential's independent public-only consumer also passed. Publication, public indexing and public consumption are complete for both releases.

All six Flourish packages share 1.1.1 in strict Core/Abstract/Culture bridge/Framework/Design/umbrella order; Essential stays 1.3.0 and Flourish WPF is excluded. The complete 1.1.1 preparation passed with exit code 0 and zero warnings/errors: Core 367, Blazor 373/373, bridge 12, Gallery 7,234 including 124 actual-event language/state checks, Node 66, CSS 21/194, launcher 95, catalog 21,217 and four consumers totaling 129 checks. Evidence: Flourish/artifacts/culture-release-1.1.1.log and Flourish/artifacts/package-consumers/546b4983f8404423b078f00b5526ab74.

## Superseded limits and preserved history

Both earlier Flourish v1.1.0 workflow attempts failed the isolated CSS fixture and skipped publication; that tag was preserved. The fixture's default-cache source assumption was corrected without changing production CSS or weakening assertions. Successful run 37540483235 supersedes historical recovery/bug-report limits that awaited CI/tag-run confirmation. The final HTTP 200 indexes and public-only 129-check consumer also supersede the pending public indexing/consumer limits. User-operated visual acceptance remains separate. Existing bug reports and append-only records remain unchanged.

## Documentation and configuration

Cleaned the active guides around current release facts, exact package scope/dependencies, scoped single-Culture.json usage in en-US/zh-CN/pt-BR and future immutable-tag release rules. Both workflows use vars.NUGET_USER || secrets.NUGET_USER in environment nuget; profile Evigila is configured and no local long-lived API key is required. The successful logins/uploads establish authorization for these exact releases. Verification documents retain previous results as historical snapshots.

## Manual acceptance and task boundary

User-operated checks remain original Gallery startup/icons/styles/layout, same-page translation and state retention, representative controls, independent culture scopes, formatting culture and cookie reload persistence. No Computer Use acceptance was performed. This documentation task modifies only the two repositories' active release guides/verification documents and this still-uncommitted round's record. It does not edit source/scripts or previously committed history. The published tags retain their original source commits. Final technical documentation and the public-verification helper correction will be committed separately under the user's existing commit/publication authorization; they do not create a new release tag or republish packages. Future commits and publication follow AGENTS.md and the user's task-scoped authorization.

## Final public-consumer configuration correction

The first public verification attempt used the same NuGet.org URI under two source names. NuGet collapsed the duplicate source and a Flourish.*-only mapping excluded Microsoft.AspNetCore.App.Internal.Assets 10.0.11, producing NU1100. Failed fixture Flourish/artifacts/package-consumers/d1afce7bb6784b64be6730553c6e64a3 preserves the original NuGet.Config, FrameworkOnly/obj/project.assets.json and diagnostics. The coordinating task added -PublicSource to Test-BlazorPackageConsumers.ps1: one public source, wildcard mapping and rejection of local package directories. Default CI/local modes and published nupkg contents are unchanged. Final public verification passed all four modes and 129 checks at Flourish/artifacts/package-consumers/027abe99e60f46dbaa1d15a75b670ff6; log Flourish/artifacts/public-culture-release-1.1.1.log.

Verified source APIs before finalizing the translation guidance: Generator emits a static Key class with string-token properties. TextKey is Gallery's using alias, not a runtime key type. ILocalizationService accepts string tokens through Parse/ParseFrom and TryParse/TryParseFrom; Culture is a string and FormatCulture is CultureInfo. Single-Culture.json ownership and three-language render-time resolution are unchanged.

## Gallery conventional public build

Gallery's conventional bin/obj Release build also passed public restore/build with TreatWarningsAsErrors=true: zero warnings/errors. Command: dotnet build src/Gallery.Flourish.Blazor/Gallery.Flourish.Blazor.csproj -c Release --configfile artifacts/package-consumers/027abe99e60f46dbaa1d15a75b670ff6/NuGet.Config -p:TreatWarningsAsErrors=true. Evidence: Flourish/artifacts/gallery-public-build-1.1.1.log. This verifies the normal build-output path; Gallery was not launched in a GUI and no actual browser/manual acceptance is claimed.
