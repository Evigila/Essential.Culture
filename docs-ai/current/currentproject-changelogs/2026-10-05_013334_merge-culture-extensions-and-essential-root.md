# Merge Culture extensions into Flourish and establish Essential root

Date: 2026-10-05 01:33:34 America/Sao_Paulo. This timestamp records the verification/report preparation window.

## Authorized scope

The user explicitly authorized folder/resource management across Flourish, Extension and Essential.Culture, requested two routes instead of three, and specified the two new Flourish.Extensions.Culture project names. Existing uncommitted work was retained. No commit or push was requested for this task.

## Changes

- Migrated all 23 maintained Extension resources: two libraries, two test projects, solution/props/ignore/workflow and existing human/AI guides. Renamed project/assembly/package/namespace/test identities and preserved method bodies and data. Added root/module solution entries.
- Renamed the Essential.Culture checkout to Essential with its Git directory intact; original HEAD e03775a7f2baa940c46d09825f6c46cc2af19067 and remote were preserved. Nested the six libraries under src/Essential.Culture, created Essential.slnx, scoped Culture version/README settings and updated tests/demo/generator paths.
- Gallery now references its in-repository extension and uses the shared EssentialCultureRoot/UseLocalEssentialCulture source-versus-package configuration for the external module and generator.
- Preserved the independent dependency direction. Framework does not require Culture. Essential does not depend on Flourish. WPF remains singleton/Dispatcher-based and Blazor remains scoped.
- Migrated CI to the new paths and package identities. Flourish extension publishing uses culture-v tags and requires matching Essential source plus external Trusted Publishing authorization. Nothing was published.
- Preserved existing human README contents under appropriate human-document ownership; Flourish maintained source remains README-free. Active docs and full directory maps were updated. Existing change records were not rewritten.
- After verifying every migrated resource and passing tests, retired the old Extension root into ignored Flourish/artifacts/solution-migration/Extension. The manifest records original and new hashes, and all original backup hashes match. No recursive delete was performed.

## Evidence

- Essential root Release build: zero warnings/errors; 89/89 tests; all five demos built; six original package IDs packed at 1.3.0. Analyzer, README and package dependency metadata checked.
- Flourish root Release build: zero warnings/errors. Core 367/367; WPF 901/901; Blazor 118/118; WPF extension 5/5; Blazor extension 12/12. Total across repositories: 1,492 passing tests/checks.
- One initial WPF test failed because its repository package allowlist omitted the two requested new packages. The expected inventory was extended, leaving all compatibility-artifact prohibitions intact; the complete WPF suite then passed.
- Extension module Release build and two renamed 1.0.0 packages passed. Blazor direct package dependencies remain Abstract 1.1.0 and Culture.Blazor 1.3.0; local WPF pack uses WPF 1.1.0 and Culture.Wpf 1.3.0, while explicit WPF package mode still evaluates its existing 1.1.0 default.
- Existing headless Gallery integration checks passed: negotiated SSR, isolated circuits, translated page/chrome/defaults, independent Brazilian formats, navigation expansion, command routing, request restoration on reload, Framework-only host, no exceptions or failed assets. Verification hosts were stopped. Computer Use was not used.
- Directory inventories match maintained source: Flourish 925 files/139 directories and Essential 101 files/30 directories, excluding stated documentation/generated areas. Old active source references and maintained Flourish READMEs are absent.

## Limits and next steps

Old extension package identities and the WPF extension namespace are deliberately replaced; desktop callers must update references. Essential public APIs and package identities are retained. NuGet-only verification depends on unpublished Culture 1.3.0 and local framework contract work reaching a feed; no public availability is claimed. Push Essential changes before Flourish CI relies on its new source paths. The historical GitHub Essential repository name is unchanged. Real desktop window acceptance and final Visual Studio startup remain on the manual checklist in solution-organization.md.
