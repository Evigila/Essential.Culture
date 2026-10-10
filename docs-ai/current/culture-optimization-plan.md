# Culture resource optimization proposal

**Status:** Approved design, with stages 1 and 2 implemented in the 1.4.0 working-tree candidate. Stages 3 and 4 remain optional. The baseline observations below describe revision `4955616` inspected on 2026-10-09 (America/Sao_Paulo, UTC-03:00); current contracts are in [resource configuration](culture-resource-usage.md) and the [API inventory](culture-public-api.md), and executed verification is recorded separately.

## Requirements and evidence

Preserve the existing key-centred, single-file multilingual editing model; allow unwanted languages to be disabled before their translation objects are loaded; add optional functional modules such as `Culture.Changelog.json` and `Culture.MainPage.json`; preserve efficient lookups while measuring startup, switching, memory, and incremental build costs separately.

The supplied [shared conversation](https://chatgpt.com/share/6ac97fb2-4d40-83e9-a238-91cf8ebcbb7b), titled “比较多语言性能”, was opened in the Codex browser. The web retrieval tool returned a cache miss; its complete public HTML response was retrieved directly and its streamed conversation data decoded. The three substantive user messages concern single-file performance, memory savings through language exclusion, and functional module files. No Computer Use was used. Conversation recommendations were checked against this checkout, rather than assumed to describe implemented capabilities.

Source evidence:

- [LocalizationCatalog](../../src/Essential.Culture/Essential.Culture/LocalizationCatalog.cs): immutable validated translations, complete JSON DOM parsing, per-key culture dictionaries, and two lookup dictionaries per selected culture.
- [LocalizationContext](../../src/Essential.Culture/Essential.Culture/LocalizationContext.cs): independent UI/formatting selection, atomic state snapshots, token/raw lookup, and generic formatting overloads.
- [LocalizationRuntime](../../src/Essential.Culture/Essential.Culture/LocalizationRuntime.cs): lazy desktop facade with one output-directory `Culture.json`, initial and fallback `en-US`.
- [CultureGenerator](../../src/Essential.Culture/Essential.Culture.Generator/CultureGenerator.cs): exactly one file named `Culture.json`, key generation, optional XAML facade, and content-based incremental equality.
- [Generator targets](../../src/Essential.Culture/Essential.Culture.Generator/buildTransitive/Arkheide.Essential.Culture.Generator.targets): root-document creation, inclusion, and copying.
- [LocalizationBuilder](../../src/Essential.Culture/Essential.Culture.Blazor/LocalizationBuilder.cs) and [LocalizationSession](../../src/Essential.Culture/Essential.Culture.Blazor/LocalizationSession.cs): existing multiple independent catalogs and scoped selection allowlist.

The complete current API inventory is in [culture-public-api.md](culture-public-api.md). The API candidates below preserve the original implementation-review proposal; the inventory identifies their final implemented signatures.

## Baseline behavior and costs

| Concern | Current evidence | Consequence |
| --- | --- | --- |
| Editing | `key -> culture -> string` in one document | Convenient translation comparison and consistency checking; preserve it inside every optional module |
| Loading | `JsonDocument.Parse`, all strings and `CompositeFormat` instances materialized | Retained memory grows with all keys and all declared languages; temporary DOM also increases peak memory |
| Lookup | One immutable state snapshot and one `FrozenDictionary` lookup on a successful raw/token path | File count does not determine steady-state lookup performance; no file I/O or JSON parsing here |
| Selection | First selection scans all keys and constructs raw/token dictionaries under a catalog-wide lock | Cold switching has linear work in key count; warm declared-language selections reuse cached tables |
| Parent/custom requests | Only explicitly declared culture names enter the selection cache | Equivalent requests such as `fr-CA` and `fr-FR` can repeatedly build identical tables when `fr` is the effective translation culture |
| Generator | Entire document content participates in equality before key generation | Translation-only edits rerun generation even when the public key shape is unchanged |
| Validation | Runtime verifies language sets, fallback, nonempty text, normalized duplicates, and format placeholders; generator mainly verifies JSON/root keys | A successful build does not currently establish complete translation validity |
| Blazor | `AddSupportedCultures` rejects unsupported selections; catalogs are already fully parsed | Existing selection restriction does not remove unwanted translations from memory |
| Multiple catalogs | Blazor supports explicit catalog IDs and independent key spaces | This is useful existing functionality, but does not make multiple physical files one generated logical catalog |

No benchmark project or measured performance results were found. “Always optimal” must be treated as regression requirements across separate metrics, rather than a claim that a single strategy wins for every workload.

## Recommended architecture

Use **one logical catalog with optional physical modules**. Keep the legacy single-file path as the default. Each file retains the current multilingual document format:

```text
Culture.json                 -> common/startup keys in all authored languages
Culture.MainPage.json         -> main-page keys in all authored languages
Culture.Changelog.json        -> changelog keys in all authored languages
Culture.SecondPage.json       -> second-page keys in all authored languages
```

Keys remain globally unique inside this logical catalog. `Key.MainPage_Title` still produces `Key.MainPage_Title`; `Localizer.Parse(Key.MainPage_Title)` and existing XAML bindings keep their shape. Cross-file duplicates are build errors with both source locations, never order-dependent overrides. Do not introduce `Key.Module.Name` tokens: the existing token suffix contract accepts ASCII C# identifiers, not dotted module paths.

Keep existing independent Blazor catalogs for applications that need isolated key spaces. A physical module is not automatically another `AddCatalog` registration. Otherwise every scope and switch would create additional contexts and dictionaries merely because source files were split.

### Language policy and resource retention

Introduce an optional immutable startup policy, shared by core loaders, contexts, the desktop facade, and the Blazor registration. Separate:

1. **Declared cultures:** names present in the authored resources.
2. **Selectable cultures:** names the host permits as UI selections.
3. **Loaded translation cultures:** permitted declared data needed by selectable requests, their permitted parent chain, and the required fallback.
4. **Formatting culture:** the independent numeric/date formatting choice already supported by core contexts and Blazor.

Normalize all policy entries with the existing culture-name rules before comparison. Define exact-tag disable semantics in the first version: disabling `fr` does not automatically disable selecting `fr-CA`, but `fr` must not be loaded or used through its parent chain. A requested allowed tag may be absent from the resources and still use a permitted parent or fallback, preserving Blazor's existing supported-custom-tag use case. A future family-wide rule must be explicit.

If allow and deny lists are both supplied, reject contradictory normalized entries during initialization. With an allowlist, omitted names are unavailable; without one, declared names except denied entries are selectable. An explicitly disabled request must fail in `SetCulture`, not merely disappear from a selector and then silently use fallback. `CultureName.Normalize` alone does not enforce this policy.

The fallback is required for every key. Reject a configuration that disables its storage; the host must first choose another valid fallback. Load only permitted declared parents that are actually necessary, and the fallback. Do not blindly equate selectable requests with retained cultures: an allowed `fr-CA` request may require `fr` data even if `fr` is not offered by the UI. Conversely, an explicitly denied parent must not sneak back into the loaded closure.

Preserve legacy behavior when no policy is configured: all translations load, `AvailableCultures` reports declared cultures, and valid undeclared culture requests may fall back. New configured contexts may report only selectable cultures through their available list, but provide a separately named declared-culture view; document this opt-in distinction. Do not silently redefine existing unconfigured behavior.

Start with startup-time exclusion. Runtime unloading is a different feature: contexts, selection caches, and in-flight snapshots can retain translation references after a list entry is removed. Dynamic replacement would require an atomic new catalog generation, migration of affected selections, clear notification semantics, and delayed reclamation. It is not required for the initial implementation.

### Parsing and validation

First add filtering to the existing DOM loader to avoid `GetString`, `CompositeFormat`, and retained translation objects for excluded languages. This offers a small compatibility surface and directly addresses retained memory. It still reads and parses all file bytes and creates a temporary DOM.

Then compare it with a buffered `Utf8JsonReader` implementation. Avoiding a DOM can reduce peak allocation, but does not make skipped JSON bytes disappear from I/O or syntax processing. Correctly handle UTF-8 BOMs, escaped names/strings, partial tokens, preserved unread bytes, reader state, maximum token size, stream position, and caller ownership. The [official streaming reader guidance](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/use-utf8jsonreader) documents the partial-buffer requirements.

Retain full authored-resource validation by default, including excluded languages, until an explicit build-validated deployment mode exists. Validation may use temporary values and discard them; exclusion guarantees concern retained translation state, not a promise of zero temporary work. An optimized selected-language validation mode may be offered only with a documented contract: build tools fully validate all authored languages, runtime always checks syntax, keys, language identities/duplicates, manifest consistency, retained text, fallback, and placeholders. User-provided `FromFile`, `Load`, or `FromJson` resources must not automatically be treated as matching a previously validated manifest.

Within each module preserve consistent per-key language sets. Different modules may declare different optional cultures; each must provide the required fallback, and each must respect the global policy. The global declared set is a documented union, while selection resolves missing optional module languages through the permitted fallback chain. This is an intentional extension that needs explicit tests rather than concatenating modules into today's strict single-document validator.

### Module discovery, loading, and lookup

Discovery is a build/startup operation. Use declared MSBuild items or a bounded opt-in `Culture.*.json` discovery rule; do not recursively scan deployment directories during translation lookup. Supply deterministic module IDs and deployment-relative paths. Diagnose duplicate IDs, normalized paths, keys, and generated hint names. Validate deployment paths and manifest/resource key consistency so an edited or missing module cannot silently misroute a key.

Generate or explicitly register a shared `key -> module/local slot` manifest without translation text. It lets an unloaded module be located without opening every file. Runtime-only callers that do not use the generator must either load/merge all supplied files or supply an explicit manifest; lazy discovery cannot infer unknown keys without reading resources.

Provide two explicit operational modes:

| Mode | Behavior | Suitable objective |
| --- | --- | --- |
| Preload all enabled modules | Load and validate before first render; retain the existing direct lookup path | Predictable lookup latency and small catalogs |
| Preload common modules, explicitly ensure others before rendering | Core/common modules start ready; route/page initialization awaits an unloaded module once | Lower retained memory and startup work for large modular applications |

Do not hide blocking file or network I/O in an ordinary synchronous `Parse`. A candidate `EnsureModuleLoadedAsync` operation belongs in host/page initialization, with cancellation and clear failure semantics. Legacy unconfigured `Parse` keeps its behavior. In opt-in deferred mode a known but unloaded module must have a documented result (for example a dedicated not-loaded exception); it must not be confused with an unknown key. Components render only after their module is ready. Blazor SSR/Interactive Server must obtain resources through an explicit host loader, and no WebAssembly support should be claimed without a separately verified client dependency and loading path.

Concurrent requests share one module load; caller cancellation must not poison a shared load needed by other callers. A failed load remains distinguishable from an unknown key and may be retried through an explicit bounded policy. Publish immutable loaded data atomically. A lookup uses one selected-language snapshot and one immutable module publication; modules use the same requested culture and policy. Existing `Changed` handlers signify selection changes, not an automatic guarantee that arbitrary later module arrivals will rerender every component. Await module readiness or introduce a separately designed readiness notification.

Avoid rebuilding a complete global frozen dictionary whenever another module loads. Prefer immutable manifest slots and shared module tables, or measure a hybrid hot-key table. Never loop through all modules per lookup. Constant lookup count is a requirement; it does not imply equal nanosecond latency for a one-lookup legacy path and a routed path.

Do not automatically unload modules in the initial version. Lazy loading defers memory growth; once every module has been visited, all cached modules may remain resident. A later eviction design needs ownership, in-flight reference, and reload behavior first.

### Generator and MSBuild

Restructure the incremental graph as:

```text
each AdditionalText + identity/options
    -> parse and validate that file
    -> value-equatable key shape + separate translation/diagnostic model
    -> global duplicate/configuration validation
    -> stable Key/CultureKey/Localize output and module manifest
```

Parse per file before `Collect`. Translation-only changes revalidate that file but leave the equatable key shape unchanged; unrelated modules reuse their previous parsed models. Changes to key ownership update the module manifest even if the global key set is unchanged. Use content equality for collections, not default array reference equality. This follows the [Roslyn incremental generator cookbook](https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.cookbook.md).

Keep the existing five MSBuild properties and their legacy aliases. Add separate opt-in module declarations/metadata. Do not place policy metadata in the root of a traditional translation document where it would be parsed as a translation key. If a `Culture.options.json` format is eventually adopted, explicitly exclude it from a `Culture.*.json` resource glob. A configuration file is optional, and neither a new format nor a new dependency is implemented by this proposal.

Update buildTransitive assets together: file inclusion, explicit linked inputs, automatic creation, output/publish paths, transitive generator delivery, and removal of stale module output after source deletion. Detect module declarations before automatically creating a new root template. Test root `Culture.json` and explicit linked files independently. Generator disablement and file auto-inclusion remain separate switches.

Keep deterministic generated type shape and namespace behavior. Current `CultureKey` values are implicit ordinal positions and can change when keys are added; do not turn them into persisted module IDs or runtime wire identities. Any dense index used for optimization should be an internal implementation detail. Avoid multiple module initializers racing to configure the global facade; build/register once explicitly before any facade subscription or lookup. A candidate one-time desktop `Configure` operation must reject attempts after first use and must not unexpectedly perform the current lazy default initialization while configuring.

### Performance improvements to compare

1. Cache selection by the effective declared translation culture, instead of the arbitrary requested name. With today's consistent per-key language set, equivalent parent/fallback requests can share one selection. In the modular extension use a bounded module/effective-culture key; preserve the requested UI name and formatting culture in each context. No unbounded cache keyed by arbitrary user tags.
2. Move cold table construction out of a single catalog-wide critical section using coordinated per-selection publication. Keep successful warm lookups lock-free. Avoid duplicate expensive construction and measure contention under concurrent scope creation.
3. Compare a shared raw/token-to-index map plus language arrays with the existing per-key culture maps and two frozen selection maps. Never assume that halving indexes automatically improves lookup latency. Retain the current path when the alternative fails its memory/latency objective.
4. Reuse translation selection for formatting-only changes. Reduce Blazor rebuilding of all catalog contexts when shared data already exists. Consider core-equivalent generic formatting overloads for Blazor only after allocation measurements, preserving the current interface for external implementations.
5. Measure DOM filtering against streaming filtering. Retained text/format objects should grow with loaded module keys and retained languages; global key/manifest metadata, caches, temporary parsing, and per-scope state are additional costs.

`FrozenDictionary` is designed for infrequent construction and frequent lookup; its construction cost supports measuring cold selection separately from hot lookup ([official documentation](https://learn.microsoft.com/en-us/dotnet/api/system.collections.frozen.frozendictionary-2?view=net-10.0)). Optional compiled deployment resources can be explored later if measurements justify a versioned manifest/resource format. They change rebuild and publication requirements and cannot be assumed faster without comparison. Do not add BenchmarkDotNet, parser packages, or new SDKs without dependency-specific authorization; initial measurements can use existing runtime facilities.

## Compatibility and delivery sequence

Original additive API candidates; factories, options and Configure are now implemented with the exact signatures in the API inventory. The stage-3 loader remains unimplemented:

| Candidate | Intended purpose and compatibility rule |
| --- | --- |
| `CatalogLoadOptions` | Immutable startup policy: selected/disabled culture names and validation mode; default behavior is the existing full strict load |
| `LocalizationCatalog.FromJson(string json, string fallbackCulture, CatalogLoadOptions options)` | Apply startup policy without changing the existing two-argument overload |
| `LocalizationCatalog.Load(Stream stream, string fallbackCulture, CatalogLoadOptions options)` | Filter retained data while preserving stream ownership and position rules |
| `LocalizationCatalog.FromFile(string path, string fallbackCulture, CatalogLoadOptions options)` | Apply policy to explicit files; no implicit trust in build-time validation |
| `LocalizationCatalog.FromFiles(IEnumerable<string> paths, string fallbackCulture, CatalogLoadOptions options)` | Stage-2 eager composition into one logical catalog with cross-file diagnostics |
| One-time `Localizer.Configure(...)` | Configure the desktop source/policy before the first lookup or subscription; existing code that never calls it retains the lazy single-file default |
| A stage-3 loader's `EnsureModuleLoadedAsync(moduleId, cancellationToken)` | Explicit readiness before rendering; the owning resource store publishes immutable module data, while individual contexts retain independent selection |

The stage-3 resource store must be designed separately from today's fully materialized `LocalizationCatalog`. Eager factory results remain immutable. Deferred storage can publish new immutable module records without modifying an existing translation record, but its lifetime, context attachment, and readiness contract need implementation review. Do not describe adding an async method alone as sufficient to make existing contexts observe newly loaded data. Avoid new overloads with a nullable options object in the existing fallback-string position, which could make existing `FromJson(json, null)`-style calls ambiguous.

| Stage | Concrete deliverable | Acceptance gate |
| --- | --- | --- |
| 0 | Record existing API, behavior, and Release benchmark baseline | Existing regression tests pass; baseline metrics reproducible |
| 1 | Core startup language policy, retained-language filtering, bounded effective-selection cache | Default path unchanged; no policy bypass through normalization/parent fallback; disabled data absent from retained catalog; memory comparison recorded |
| 2 | Optional module items, per-file generator pipeline, global duplicate diagnostics, eager logical-catalog loader | Existing tokens/XAML/DI usage preserved; multi-module build/publish/package-consumer checks pass |
| 3 | Explicit asynchronous module readiness and common-module preloading | No I/O in ready-module lookups; concurrent loads deduplicated; request/circuit isolation and error/cancellation cases verified |
| 4 | Measured streaming/index/compiled-resource alternatives | Adopt only candidates that meet stated startup, memory, lookup, switch, and build budgets |

Stages 1 and 2 solve the requested feature gaps and are implemented in the candidate. Stages 3 and 4 remain optional for workloads that demonstrate a need. The candidate uses an eager file manifest rather than a key-to-module routing manifest, because there are no unloaded modules to locate. Strict filtering discards excluded objects after temporary validation; it does not skip their validation allocations.

Preserve assembly identities, package IDs, existing overloads, error/fallback semantics in the legacy path, stream ownership, independent formatting, scoped isolation, and the current successful lookup allocation profile. Adding members to `ILocalizationService` must consider third-party implementers; use an additive extension/interface strategy or an explicit breaking-version decision. Do not promise event ordering beyond today's synchronous, lock-external `Changed` contract.

## Verification performed and planned

Performed on this working tree: source/configuration/test inspection, shared conversation extraction, documentation inventory and dependency resolution. `dotnet test Essential.slnx -c Release --artifacts-path artifacts/documentation-audit --no-restore --verbosity minimal` passed **106 tests**: core 61, generator 20, Blazor 21, WinUI marker tests 4. These verify the existing baseline, not the proposed capabilities. Full desktop UI behavior and benchmarks were not run.

Future regression checks must cover:

- Legacy one-file use; multiple optional modules; duplicate keys/IDs/paths; different optional language sets; missing fallback; bad/empty translations and placeholder mismatches even in excluded languages under strict validation.
- Normalized allow/deny lists, custom requests, explicit denied parents, fallback disablement, all selections disabled, initial culture rejection, and direct API selection bypass attempts.
- Raw/generated tokens, missing/invalid tokens, 0/1/2/3/N format arguments, formatting-only switches, scoped isolation, consistent snapshots during concurrent switches, and notification/no-op behavior.
- Single-module incremental edits, translation-only edits, key moves between modules, add/remove/reorder of modules, stable output names, resource/config hash mismatch, stale published file cleanup, and generator-disabled builds.
- Concurrent module load success/failure/cancellation; readiness before render; missing deployment resources; unloaded-known-key semantics; package-only consumers for all affected adapters.

Release measurements should cover representative 100/1,000/10,000 key datasets, 2/10/30 languages, and 1/10/100 modules without assuming every combination matches a real product. Measure cold load and first selection, allocation and post-GC retained memory, peak working set, hot raw/token hits and misses, format arities, warm/parent/custom switches, concurrent scopes, and no-op/translation-only incremental builds. Report dataset, runtime, OS, warmup, samples, dispersion, and baseline side by side. A successful no-format lookup should not add per-call allocation; formatted result strings inherently allocate.

Manual checklist after implementation (not claimed complete now):

1. Run the Console/WPF/Avalonia/WinUI Galleries with their legacy single document and compare all texts before/after a language switch.
2. Configure a denied language: verify it is absent from selection controls and a direct normalized `SetCulture` request fails without changing state; verify fallback configuration failures are clear.
3. Split keys into functional files, then open each feature: verify common text, lazy readiness, format parameters, dynamic key binding, and dialog/popup text.
4. Exercise multiple desktop windows, background language changes, delayed data contexts, repeated open/close, and WinUI Attach/Detach/Apply/Dispose behavior on owning UI threads.
5. Use two independent Blazor sessions: switch one, confirm the other is unchanged; check SSR/interactive initial selection, host persistence, encoded text, and disposed components.
6. Inspect build/publish output and a clean package consumer: all declared modules are present, excluded deployment data follows configuration, removed modules do not remain, and linked paths are correct.
7. Compare startup, first-feature opening, warm switching, and memory after visiting all features against the recorded Release baseline.
