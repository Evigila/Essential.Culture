# Project documentation and instruction compliance audits

## Current framework 1.0.0 integration audit

Inspected on 2026-10-09, America/Sao_Paulo (UTC-03:00), against repository baseline `75548c04715624b55206194915e79bb8d3a0908a` and the user's newly installed AGENTS control framework. The current router requires the repository skills in `.agents/skills/`, the manifest/installation lock, synchronization launcher and script, and schema-2 audit state tied to the installed framework version. The preceding schema-1 state lacks `auditedFrameworkVersion`, so the new gate requires a full audit even though its completion time is recent. The prior completion was `2026-10-10T00:15:54Z`; the question time remains `2026-10-06T20:24:11Z`.

The installation manifest identifies framework **1.0.0** from `Evigila/AGENTS.md`; its consumer lock records source commit `8f02f0f93a906c925d8f8a466887a7aed3c2b08a` and normalized manifest digest `ca5a058a2960b279eaf62d7fcf4c978ca064ff9a3e528485e8f0a315744432ee`. All ten managed payload files and the manifest match their recorded normalized SHA-256 hashes. Seven seed entries preserve existing project facts/audit state and initialize only missing files. The two installed skills have scoped manifests and valid rule references. The synchronization script was inspected for manifest/path validation, conflict handling, seed preservation and check-only behavior; managed framework files were retained without local customization.

The full audit verifies required paths/content, the complete maintained tree, project/source boundaries, declarations and available local/public resolution evidence. [Architecture](1_architecture.md) and [dependencies](1_dependency.md) are synchronized with the installation. `fetch-agents.bat -Check` successfully resolved the remote source commit above, reported Installed/source 1.0.0 and Up to date, and exited 0 without changing any managed-file/lock bytes (`artifacts/agents-control-check.log`). Local framework backups are preserved and excluded by `.agents/backups/` in `.gitignore` and the tree's explicit temporary-file scope. Older records, human documentation and product READMEs are preserved. Final verification results and the schema-2 completion are recorded in [the integration record](1_changelogs/2026-10-09_224710_integrate-agents-control.md).

This is a documentation/control integration audit. Culture source, package versions, build/release configuration, public APIs and the published `v1.4.0` tag are unchanged. Existing 201-test/local-package/public-consumer results remain dated evidence; functional tests were not rerun for unchanged product behavior. No Computer Use was performed. Manual framework checks are listed in the integration record; product UI acceptance continues to use [the existing checklist](culture-resource-usage.md#manual-acceptance).

Existing compatibility-sensitive DI/global/fixture namespace gaps, Gallery Flourish integration gaps, human Gallery snippets and historical timezone limitations below remain unresolved. A completed documentation audit verifies inventories and records these gaps; it does not certify every implementation against every standard.

## Historical 2026-10-09 pre-framework audit and release follow-ups

**Inspection date:** 2026-10-09, America/Sao_Paulo (UTC-03:00). Baseline `49556163d2e72fe8ffa27cf5f942149823844bb0` plus the user's pending instruction/documentation migration and README changes.

**Baseline result:** The forced analysis audit repaired and checked the required documentation structure and inventories. It did not implement features or certify all pre-existing source/Gallery/history compliance. The dated baseline results below are retained as audit evidence.

**Implementation follow-up:** The user subsequently approved the optimization design and release-file preparation. Stages 1/2 now provide language policy, retained filtering, eager modules, incremental validation and one-time facade configuration; candidate version is 1.4.0 and active release identity is Evigila/Essential. New owned code/fixtures use ArkheideSystem namespaces and semantic type names. Package README changes describe the approved features and repair affected example links while preserving the user's earlier edits. Source branding summaries/titles touched by the change have been corrected. [Architecture](1_architecture.md), [dependencies](1_dependency.md), [API inventory](culture-public-api.md), [performance](culture-performance.md) and [release verification](release-verification.md) carry current facts. The completed audit timestamp is not refreshed by ordinary implementation work.

The remaining compatibility-sensitive DI namespace, older fixture/global namespaces, Gallery UI integration, stale human Gallery snippets and historical timezone limitations still require their own scope/decision. They are not certified compliant by these feature changes. Physical UI acceptance remains manual; the recreated remote Trusted Publishing policy was subsequently validated by the release follow-up below. See [the current checklist](culture-resource-usage.md#manual-acceptance).

**Release follow-up:** The user explicitly authorized maintaining only the module English/Simplified Chinese READMEs and then committing/tagging/publishing. The two existing READMEs were corrected without adding other READMEs. The v1.4.0 source commit and tag were pushed; the new repository policy accepted OIDC login and all six uploads. Physical UI acceptance remains manual. [Release verification](release-verification.md) separates upload, public indexing and public-only consumption. The audit completion timestamp remains unchanged.

## Audit trigger and ownership

The user explicitly required a full check after updating AGENTS.md. The state also independently triggered it: schemaVersion=3 is unsupported by the effective schema-1 rule. Both new inventory files were unverified templates, and `1_archived/` was absent. The earlier completion marker could not justify SKIP_AUDIT under these circumstances.

Read the root router, complete shared rules/code/UI standards, current structure/dependency facts, relevant source/configuration/tests, and relevant current/history evidence. All missing essential inventory information was available in the repository or existing dependency resolution. No new missing-material question was needed. Existing pending human changes were retained. Human README and nested usage guides were read and findings reported; no human prose was edited by this task. The root `docs/.gitkeep` was verified to be empty.

The user's task instruction requires `ArkheideSystem` namespaces. The shared code standard additionally permits `Arkheide.`, but that allowance does not relax the user's narrower requirement for this task. Proposed new type names describe responsibilities and do not carry product/organization branding. No namespace or package migration was performed.

## Baseline requirement-by-requirement result

| Effective requirement | Verified result |
| --- | --- |
| Root AGENTS.md exists and routes every task | Present, inspected, task-specific readings followed; no router rewrite needed |
| Audit gate/schema | Full audit performed; legacy state preserved in [archived state](1_archived/2026-10-09_audit-state-schema-3.json); completion state migrated only after final verification |
| Required directories and assigned document subjects | All 16 required/implied paths checked, including root docs/docs-ai, common/current, required inventories and skills/history/bug/archive directories; empty markers verified |
| Compact router/common standards hold effective guidance | Inspected; no dated approval or superseded policy narrative found in active shared instructions |
| Complete repository architecture tree | [1_architecture.md](1_architecture.md) contains all 114 maintained files under concrete exclusions; independently compared parsed tree with a fresh no-ignore file enumeration, no differences |
| Architecture responsibilities and boundaries | Three solution files, 16 project declarations, runtime/resource boundaries and inward dependency directions inspected; source findings retained rather than presented as invented services |
| Complete dependency inventory | [1_dependency.md](1_dependency.md) covers direct, implicit and transitive packages, local references, SDK/runtime, services/assets/tools; 73 unique package/version pairs independently match 21 restored graphs exactly |
| Dependency evidence and unknown values | Actual manifests/assets/cache provenance used; Windows SDK download-only reference pack listed separately; unknown licenses/tool/runner versions identified rather than invented |
| No unauthorized dependency change | No package, SDK, service or asset declaration changed; restores used existing declarations |
| Documentation ownership and formats | Only AI documentation and audit state changed; prose is Markdown and machine state is JSON; root human area and existing README changes retained |
| Append-only history/provenance | All 18 user-migrated old records match their HEAD old-path blob hashes byte for byte; this task adds one record and edits none |
| Active facts/links | Fixed retired tree links in solution organization and Blazor adaptation; removed obsolete host-specific active checkout instructions; local links in active documents verified |
| Verification boundaries | Existing 106 tests passed; no Computer Use, no real desktop UI acceptance, no claimed benchmark results |
| Delivery and Git | Analysis/API inventory/manual checks delivered; no Git commit, push, tag, or publication performed; ask user about a commit using recent imperative English subject style |

## Baseline compliance and maintenance findings

| Finding | Evidence and current treatment |
| --- | --- |
| Owned DI extension namespace lacks the required prefix | `src/Essential.Culture/Essential.Culture.Blazor/LocalizationServiceCollectionExtensions.cs:3` uses `Microsoft.Extensions.DependencyInjection`. Conventional DI discovery is not a recorded exception. Moving it changes fully-qualified public identity; assess migration or obtain a scoped standards exception before changes. |
| Repository-owned generated/global namespaces need a decision | Package consumer RootNamespace is `BlazorOnly`, generated target is `PackageConsumer.Texts`; generator tests use `Demo.Generated`, and Blazor Gallery declares a global partial Program. A RootNamespace property does not automatically namespace C# top-level code. External consumer configurability is valid API behavior; repository-owned fixture choices still need assessed compliance. |
| Gallery UI versus shared Flourish integration rules | Existing desktop XAML uses native framework controls; Blazor Home.razor uses native button/select and app.css. No Flourish package or inspected implementation is present here. Do not certify full UIUX compliance or add dependencies by assumption. Determine applicable Gallery exceptions/integration boundaries from actual contracts before any UI migration. |
| Legacy changelog timezone metadata is incomplete | Filenames retain the required dated slug shape and bytes, but several original records omit explicit timezone/UTC offset. Their true recording offsets cannot be reconstructed merely from the current user timezone. This new record documents the provenance limitation without rewriting history. |
| Human Gallery package snippets are stale | Console/Wpf/Avalonia/WinUI README.md line 10 says 1.2.0; current module props and projects use 1.3.0. Reported, not rewritten without task-scoped human-document authorization. |
| Existing branding outside namespace/package identities | Localizer summary and some generator diagnostic titles say “Arkheide Essential Culture”. This is a low-priority cleanup opportunity under the user's preference, not a reason to change compatibility IDs or package names during analysis. |
| Requested feature and performance guarantees are not present today | Core has no language-disable policy or JSON module composition; Generator accepts one recognized file. Blazor already has catalog routing and UI selection allowlists. No benchmark establishes universal performance optimality. See the separately marked [proposal](culture-optimization-plan.md). |
| External/historical evidence boundaries | Cross-repository Flourish links and historical release artifacts are references, not new verification in this checkout. Cache-origin L/N and tool/license unknowns are explicit in the dependency inventory. No fresh public-only restore or external publication revalidation was claimed. |

These findings explain why the baseline project could not be described as having met every new AGENTS requirement. Features and touched branding are addressed by the follow-up above; compatibility/human/history boundaries remain explicit. Existing-name migration must be assessed, and historical unknown metadata cannot be invented. The documentation inventory can nevertheless be completed from verified evidence.

## Baseline changes and verification

Filled architecture and dependency inventories, added the missing archive marker, preserved old audit-state provenance, and corrected active tree/checkout references. Added [optimization proposal](culture-optimization-plan.md) and [complete API inventory](culture-public-api.md). The 18 handwritten public types and consumer-generated/build contracts were inspected; internal runtime/session/helper types are not counted as external APIs.

Four existing suites passed using Release configuration and isolated audit artifacts: Core 61, Generator 20, Blazor 21, WinUI marker 4 (**106 total**, no failures or skips). These establish current baseline behavior only. Root/local Gallery/package Gallery/isolated Blazor consumer dependency restores passed. No production source, project declaration, dependency identity or public API changed. No new release or full Gallery build is claimed by this task.

Independent checks: required paths/empty marker, tree equality, exact restored package coverage, historical blob equality, active local Markdown targets and git diff --check. LF/CRLF normalization notices from the user's pre-existing text changes are warnings, not whitespace-check failures.

## Audit-state migration

The old state is archived with status/reason/scope/replacement and all original field values. New schema 1 keeps only the required fields. Its prior question marker, `2026-10-06T20:24:11Z`, is preserved as `lastMissingMaterialAskedAtUtc`; the original state's note says this was the nearest recorded time after the old user's response, not a precisely recorded question instant. This historical uncertainty is retained here and in the archive. No new question occurred. The new completion field records the actual UTC instant after the required documentation repairs and checks; it is a documentation-audit marker, not a claim of runtime/UI compliance.

## Manual checks

Open the root/module/Gallery solutions and current inventories. Use the [implemented resource checklist](culture-resource-usage.md#manual-acceptance) for language-disable/module acceptance and independent Blazor sessions. Real WinUI host/window/dispatcher behavior remains manual: its four automated tests cover markers only.
