# Verify Essential.Culture 1.3.0 public release

Recorded on 2026-10-06T19:23:10-03:00 (America/Sao_Paulo).

## Completed release

Essential source commit 983c636bdba7e1a7b74cee33b5c682c9bf06364e is tagged v1.3.0. [Workflow run 37537838178](https://github.com/Evigila/Essential.Culture/actions/runs/37537838178) succeeded, including NuGet Trusted Publishing login and six Created package-upload responses. All six public flat-container indexes returned HTTP 200 and list 1.3.0: Generator, Core, Wpf, Avalonia, WinUI and Blazor.

The fresh-cache, public-source-only consumer at artifacts/public-consumers/blazor-1e4f292745bf4a2fbe68954d77b4132a restored, built with warnings as errors and ran successfully. It directly references only Arkheide.Essential.Culture.Blazor, obtains Core/Generator transitively and verifies generated keys, independent scoped languages, formatting culture and encoded rendering. Release preparation passed 106 checks (61 Core, 20 Generator, 21 Blazor, 4 WinUI) and five Gallery builds with zero warnings/errors.

## Documentation changes

Updated the active release guide and verification document with published-package evidence and the current environment username contract: vars.NUGET_USER || secrets.NUGET_USER. NUGET_USER is configured, the user confirmed profile Evigila, and no long-lived local API key is required. The successful six-package release establishes runtime authorization for this release, not unrelated future package IDs. Prior preparation/configuration notes retain their historical scope.

## Separate Flourish boundary

Flourish is not published. Both v1.1.0 workflow attempts failed CSS verification because a fixture lacked the default package cache, and both skipped publication. A later reproduction encountered NU1301. The coordinating task is fixing that path and advancing Flourish to 1.1.1 without overwriting the v1.1.0 tag. Essential publication does not imply Flourish publication.

## Verification and manual acceptance

Reviewed documentation links, exact source/tag/run/package identities and the public-consumer evidence supplied by the coordinating task. This follow-up changes only the two active Essential release documents and this new append-only record; it does not modify source/scripts/old history or create a commit, tag, push or upload. Manual checks remain independent Blazor sessions, UI/format culture, cookie reload persistence and desktop language refresh. No Computer Use testing was performed.
