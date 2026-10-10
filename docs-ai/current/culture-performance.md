# Culture resource performance evidence

Measured on 2026-10-09 America/Sao_Paulo (UTC-03:00), using .NET 10.0.12 / SDK 10.0.401, Windows 10.0.26300, x64, 16 logical processors, Release builds. No benchmark package or dependency was added. [The console probe](../../tests/Tests.Essential.Culture.Performance/Program.cs) is an optional development tool outside the production/test solutions.

The baseline is core source copied from Git revision `49556163d2e72fe8ffa27cf5f942149823844bb0` into ignored `artifacts/performance-baseline`, compiled without the generator. The candidate uses the actual core project. Both run the same maintained probe. Baseline/current JSON reports remain under ignored `artifacts/performance/`; the tables below retain their material results.

## Method and limits

Datasets contain 100 / 1,000 / 10,000 ordinary keys, plus four format templates, 2 / 10 / 30 languages and approximately 80 characters of translation payload per key. Module cases partition the same total keys among 10 or 100 files. Single-file cold loading uses `FromJson`; module loading uses `FromFiles`, so module timings include file I/O and are not a direct parser comparison.

Each cold load has seven samples after full GC; retained memory is the managed heap delta after full GC while keeping the catalog alive. Input JSON is already alive before the measurement and excluded from the delta. The probe uses a non-inlined helper to prevent a preceding sample's catalog from remaining live. Cold allocation includes temporary validation and UTF-8/DOM work. First selection is measured separately. Hot operations have seven samples and 200,000 iterations, preceded by 20,000 warmups; switches use smaller explicit iteration/warmup counts. Reports include median, sample P95 and allocation. With seven samples, sample P95 is the maximum, not a reliable population tail estimate.

These are bounded engineering measurements on a shared workstation. Tiered JIT, GC history and concurrent development activity affect timing. The complete matrix and an isolated large-case repeat are both retained; the repeat must not be used to hide the matrix's slower large-case result. No universal latency guarantee or proof of absolute optimality is inferred.

## Retention and cold loading

| Dataset | Baseline/all retained bytes | Candidate/all retained bytes | Candidate/two languages retained bytes | Candidate/two cold median ms |
| --- | ---: | ---: | ---: | ---: |
| 100 keys, 2 languages, single file | 161,832 | 163,008 | — | — |
| 1,000 keys, 10 languages, single file | 6,808,880 | 6,810,568 | 1,559,240 | 11.22 |
| 10,000 keys, 30 languages, single file | 199,662,096 | 199,665,112 | 15,493,176 | 121.57 |
| 10,000 keys, 10 languages, 10 files | Not supported | 67,940,776 | 15,487,448 | 45.99 |
| 10,000 keys, 10 languages, 100 files | Not supported | 67,932,736 | 15,485,072 | 55.66 |

Retaining two of thirty languages reduces completed catalog memory by **92.24%**. Optional modules preserve comparable retained memory at a fixed total key/language count. The small additional unfiltered metadata/cache overhead is 1–3 KB in these single-file cases.

| Single-file unfiltered load | Baseline median / sample P95 ms | Candidate median / sample P95 ms |
| --- | ---: | ---: |
| Full matrix: 100 keys / 2 languages | 0.36 / 0.53 | 0.34 / 0.48 |
| Full matrix: 1,000 keys / 10 languages | 14.66 / 15.72 | 12.63 / 13.57 |
| Full matrix: 10,000 keys / 30 languages | 443.06 / 476.44 | 555.38 / 596.10 |
| Isolated repeat: 10,000 keys / 30 languages | 494.43 / 608.63 | 400.76 / 571.61 |

The isolated repeat's filtered cold median is 110.08 ms. The full matrix's unfiltered large-case slowdown did not reproduce in isolation. Default cold-load allocation fell from 417,615,720 to 368,248,728 bytes in the isolated large case, about 11.8%, after caching repeated culture-name normalization within each document and avoiding repeated global retained-name enumeration. Strict filtered validation still allocates approximately **310.7 MB temporarily** for this dataset, despite retaining 15.5 MB. Streaming validation may be worth a later experiment for this peak-allocation workload; it is not implemented or assumed superior.

## Query and selection

Successful raw-key and `Key.*` token queries allocated **0 bytes per call** in all measured candidate cases. Hot medians varied about 8–30 ns across scenarios and repeats; the successful lookup implementation still reads one immutable state and one frozen lookup table. A valid missing prefixed token retains the existing substring-validation allocation (40 bytes in this probe). Formatting allocates result strings; this is expected, and typed one/two/three argument overloads retain their existing behavior.

At 10,000 keys, the full matrix's repeated `fr-CA` / `fr-FR` switching when both resolve to `fr` fell from roughly 3.39 ms to 0.23 microseconds per switch. Repeated custom-tag fallback switches fell from roughly 2.97 ms to 4.65 microseconds. The candidate shares a bounded effective retained-culture table rather than rebuilding all key selections for arbitrary requested names. Requested UI/formatting names remain separate. Warm successful lookup does not perform JSON parsing, directory discovery or file I/O.

The probe also exercises raw/token misses, formatting with 1/2/3/N arguments, warm switches, first selection and 100 concurrent independent contexts. Generator regression tests inspect tracked incremental steps: a translation-only edit reparses that file, keeps other files cached, and reuses unchanged generation inputs. They validate caching behavior rather than claim a measured incremental-build speedup.

## Reproduction

```powershell
$env:MSBuildEnableWorkloadResolver = 'false'
dotnet run --project tests/Tests.Essential.Culture.Performance -c Release `
  --artifacts-path artifacts/performance-current-build -- artifacts/performance/current.json
# Optional isolated repeat:
dotnet run --project tests/Tests.Essential.Culture.Performance -c Release `
  --artifacts-path artifacts/performance-current-build -- artifacts/performance/current-large.json large
```

For baseline comparison, copy the original core `.cs` files with `git show <revision>:<path>` into an ignored directory and create a `net10.0` class-library project with implicit usings/nullable enabled and `AssemblyName=Essential.Culture`. Set `BaselineCatalogProject` to that absolute project path and use a separate artifacts path. This is an explicit local source comparison, not an external dependency or deployed project.

Deferred loading, automatic module eviction, streaming parsing and alternate dense-index storage remain optional stages. The accepted candidate retains the measured direct lookup path and eager immutable catalog behavior.
