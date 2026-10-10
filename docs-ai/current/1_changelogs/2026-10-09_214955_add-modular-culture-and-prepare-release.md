# Add modular Culture resources and prepare the 1.4.0 release

Recorded on 2026-10-09 21:49:55 America/Sao_Paulo (UTC-03:00), corresponding to 2026-10-10 00:49:55 UTC. Baseline: `49556163d2e72fe8ffa27cf5f942149823844bb0` plus the user's pending instruction/documentation migration and README edits. The user approved the optimization proposal and release preparation; this record does not authorize committing or publishing.

## Implemented

- Preserve single-file multilingual JSON and existing APIs; add immutable `CatalogLoadOptions`, declared/selectable language distinction, strict validation with retained translation filtering, and matching context/Blazor startup policy enforcement.
- Add eager `FromFiles` composition and one-time `Localizer.Configure`. Share effective retained-culture selections through a bounded concurrent lazy cache; warm lookups remain immutable and perform no resource I/O.
- Add explicit `CultureModule` items and optional root-file discovery, an internal deployment manifest, file-level incremental validation, AEC006/007, safe deployment paths and bounded build/publish cleanup. Disabled generation skips parsing; host-owned inclusion remains supported.
- Set the six existing package candidates to 1.4.0 and canonical repository metadata to `Evigila/Essential`. Add Trusted Publishing repository/workflow/environment/tag/OIDC preflight and stricter package source-asset verification. Preserve existing dependency and action versions.
- Add isolated modular package consumption and an optional dependency-free performance probe. Update current architecture/dependencies, complete 19-type API inventory, usage, performance, audit follow-up and release guidance. Preserve prior append-only records, user edits and the completed audit timestamp.

## Verification

The independent complete Release preparation passed **201 tests** (89 Core, 82 Generator, 26 Blazor, 4 WinUI), with no failures/skips and zero build warnings/errors. Five Galleries passed in source mode and again in candidate-package mode using a new cache and explicit source mapping. All six candidates passed package verification. Final Generator targets were repacked after the empty-ledger refinement and matched source SHA-256; its 82 tests and a fresh modular build/run/publish/run consumer passed without warnings. Blazor-only consumption also passed. Exact evidence and earlier unsuccessful attempt boundaries are recorded in [release verification](../release-verification.md).

The maintained probe measured a 92.24% completed-catalog memory reduction when retaining two of thirty languages at 10,000 keys, and zero allocation for successful raw/token queries. [Performance evidence](../culture-performance.md) retains the slower full-matrix cold result, the isolated repeat and temporary-allocation limits; no absolute performance guarantee is claimed.

## Outstanding acceptance

No commit, tag, push or publication occurred. The recreated remote NuGet policy is verified only by the future authorized tag workflow; login/uploads, public indexing and fresh-cache public-only consumption are separate acceptance steps. Physical Gallery/Blazor UI behavior remains on the [manual checklist](../culture-resource-usage.md#manual-acceptance).

Deferred module loading/eviction, streaming parsing and alternate storage remain optional measured follow-ups. Compatibility-sensitive pre-existing namespace/UI/history findings remain explicit in [the audit](../documentation-audit.md); this implementation does not certify unrelated compliance or introduce a Flourish dependency.
