# Phase 3.3 vs Phase 3.4 performance

Environment: Apple M1, 8 logical cores, macOS 27.0.1, .NET SDK 10.0.102,
.NET 10.0.2 Arm64 RyuJIT, BenchmarkDotNet 0.15.8, `ShortRun`
(3 warmup / 3 iterations, Server GC disabled), benchmark target `net10.0`.

Both runs used the same benchmark project and job settings. This is a shared machine and
the in-run manual baseline moved between runs, so the normalized ratio is shown alongside
absolute numbers.

## Warm-cache

| Scenario | 3.3 mean / median | 3.4 mean / median | Absolute delta | 3.3 allocated | 3.4 allocated | Normalized 3.3 → 3.4 |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Flat DTO | 287.46 / 290.34 ns | 322.16 / 325.28 ns | +12.1% | 704 B | 712 B | 22.14x → 21.13x (-4.5%) |
| Nested object | 492.99 / 491.74 ns | 621.65 / 616.57 ns | +26.1% | 944 B | 952 B | 11.25x → 12.02x (+6.8%) |
| Custom ForMember | 184.45 / 184.42 ns | 224.79 / 225.95 ns | +21.9% | 648 B | 656 B | 3.84x → 4.10x (+6.8%) |
| Circular reference | 261.09 / 259.52 ns | 331.40 / 285.50 ns | +10.0% median | 520 B | 528 B | within run noise |
| Collection 10 | 2,178.31 / 2,181.01 ns | 2,412.82 / 2,413.39 ns | +10.8% | 3,256 B | 3,264 B | 18.79x → 20.72x (+10.3%) |
| Collection 100 | 19,673.40 / 19,811.75 ns | 21,380.18 / 21,326.17 ns | +8.7% | 30,160 B | 30,168 B | 21.36x → 22.62x (+5.9%) |
| Collection 1000 | 198,026.60 / 203,538.81 ns | 220,659.90 / 220,662.67 ns | +11.4% | 299,632 B | 299,640 B | 22.60x → 22.80x (+0.9%) |

Warm allocation is effectively unchanged: +8 bytes per mapped top-level operation in
these scenarios does not change Gen0/Gen1 behavior.

## Cold configuration + first Map

| Scenario | 3.3 mean | 3.4 mean | Absolute delta | 3.3 allocated | 3.4 allocated |
| --- | ---: | ---: | ---: | ---: | ---: |
| Flat DTO | 472.17 ns | 553.62 ns | +17.3% | 2,128 B | 2,160 B |
| Nested object | 830.00 ns | 956.62 ns | +15.3% | 2,944 B | 2,976 B |
| Custom ForMember | 1,359.89 ns | 1,681.30 ns | +23.6% | 4,976 B | 5,008 B |
| Circular reference | 503.60 ns | 497.19 ns | -1.3% | 1,944 B | 1,976 B |
| Collection 10 | 2,387.27 ns | 2,663.55 ns | +11.6% | 4,968 B | 5,000 B |
| Collection 100 | 20,053.78 ns | 22,109.36 ns | +10.2% | 31,872 B | 31,904 B |
| Collection 1000 | 189,549.19 ns | 224,339.05 ns | +18.3% | 301,344 B | 301,376 B |

Normalized to the manual baseline, cold flat is unchanged, cold nested is slightly better,
and cold custom/collection show roughly 7–11% overhead from exception-safe paths, depth
tracking and derived-map snapshot preparation. Cold allocation has a fixed +32 bytes per
new mapper, with no meaningful relative change.

## Interpretation

Phase 3.4 adds:

- try/catch paths for source getters, destination setters and constructors
- index-aware enumerator error handling
- MaxDepth tracking on object and collection mapping
- derived-map resolution and ambiguity handling
- opt-in configuration validation (not on the hot path)

The measured warm cost is approximately 0–10% depending on scenario and host noise, with
unchanged allocations. Relative to the Phase 3.2 baseline, Phase 3.4 remains roughly
4.2x–5.3x faster for flat/nested mapping and about 2.6x–2.8x faster for collections,
with collection allocations still around 81% lower.

Raw reports: `phase-3.3-after.md`, `phase-3.4-after.md`.
