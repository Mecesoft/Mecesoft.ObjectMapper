# Phase 3.4 vs Phase 3.5 performance

Environment: Apple M1, 8 logical cores, macOS 27.0.1, .NET SDK 10.0.102,
.NET 10.0.2 Arm64 RyuJIT, BenchmarkDotNet 0.15.8, `ShortRun`
(3 warmup / 3 iterations, Server GC disabled), benchmark target `net10.0`.

The default `MaxCollectionSize` is 100,000; all benchmark collections are 1,000 elements or
smaller, so no benchmark is limited. The only hot-path additions are the per-collection
known-count check, the per-element lazy-sequence check, and enumerator disposal bookkeeping.

## Warm-cache

| Scenario | 3.4 mean / median | 3.5 mean / median | Allocated 3.4 → 3.5 | Normalized 3.4 → 3.5 |
| --- | ---: | ---: | ---: | ---: |
| Flat DTO | 322.16 / 325.28 ns | 323.53 / 323.45 ns | 712 → 712 B | 21.13x → 21.64x (+2.4%) |
| Nested object | 621.65 / 616.57 ns | 610.41 / 611.99 ns | 952 → 952 B | 12.02x → 12.31x (+2.4%) |
| Custom ForMember | 224.79 / 225.95 ns | 216.59 / 213.01 ns | 656 → 656 B | 4.10x → 3.90x (slightly better) |
| Circular reference | 331.40 / 285.50 ns | 250.53 / 250.44 ns | 528 → 528 B | within run noise |
| Collection 10 | 2,412.82 / 2,413.39 ns | 2,151.31 / 2,148.86 ns | 3,264 → 3,264 B | 20.72x → 21.48x (+3.7%) |
| Collection 100 | 21,380.18 / 21,326.17 ns | 19,610.43 / 19,515.14 ns | 30,168 → 30,168 B | 22.62x → 23.72x (+4.9%) |
| Collection 1000 | 220,659.90 / 220,662.67 ns | 201,003.67 / 201,601.58 ns | 299,640 → 299,640 B | 22.80x → 23.68x (+3.8%) |

Warm allocations are byte-for-byte unchanged. The normalized 0–5% variation is within the
ShortRun/host-noise envelope and reflects the extra per-element limit branch.

## Cold configuration + first Map

| Scenario | 3.4 mean | 3.5 mean | Allocated 3.4 → 3.5 |
| --- | ---: | ---: | ---: |
| Flat DTO | 553.62 ns | 535.50 ns | 2,160 → 2,160 B |
| Nested object | 956.62 ns | 951.64 ns | 2,976 → 2,976 B |
| Custom ForMember | 1,681.30 ns | 1,972.70 ns | 5,008 → 5,008 B |
| Circular reference | 497.19 ns | 433.75 ns | 1,976 → 1,976 B |
| Collection 10 | 2,663.55 ns | 3,178.21 ns | 5,000 → 5,000 B |
| Collection 100 | 22,109.36 ns | 19,989.44 ns | 31,904 → 31,904 B |
| Collection 1000 | 224,339.05 ns | 199,788.73 ns | 301,376 → 301,376 B |

Cold results are noisy (the custom and collection-10 cases have large error bars), but
allocations are unchanged and the larger collection cases remain in the same range or
improved.

## Interpretation

Phase 3.5 adds bounded collection processing with no measurable allocation cost:

- 100,000-element default per-collection limit
- early known-count check before destination allocation
- per-element lazy/infinite sequence check
- nested per-collection enforcement
- safe enumerator disposal without masking a primary mapping error

Relative to the Phase 3.2 baseline, the engine remains approximately 4.2x–5.3x faster
for flat/nested mapping, about 2.9x–3.1x faster for collections, and collection allocations
remain about 81% lower.

Raw reports: `phase-3.4-after.md`, `phase-3.5-after.md`.
