# Phase 4.2 vs Phase 4.3 performance

Environment: Apple M1, 8 logical cores, macOS 27.0.1, .NET SDK 10.0.102,
.NET 10.0.2 Arm64 RyuJIT, BenchmarkDotNet 0.15.8, `ShortRun`
(3 warmup / 3 iterations, Server GC disabled), benchmark target `net10.0`.

Phase 4.3 changes configuration-time code only: atomic batch profile registration,
reverse-map rollback state, a validation marker and a private registration adapter. The
mapping hot path and its allocations are unchanged.

## Warm-cache

| Scenario | 4.2 mean / median | 4.3 mean / median | Allocated 4.2 → 4.3 |
| --- | ---: | ---: | ---: |
| Flat DTO | 306.34 / 305.66 ns | 314.78 / 315.84 ns | 712 → 712 B |
| Nested object | 548.53 / 550.79 ns | 597.28 / 600.82 ns | 952 → 952 B |
| Custom ForMember | 209.34 / 212.47 ns | 219.37 / 219.07 ns | 656 → 656 B |
| Circular reference | 290.36 / 288.24 ns | 275.68 / 270.16 ns | 528 → 528 B |
| Collection 10 | 2,209.97 / 2,204.42 ns | 2,190.28 / 2,185.12 ns | 3,264 → 3,264 B |
| Collection 100 | 20,128.80 / 20,250.17 ns | 20,863.57 / 20,808.73 ns | 30,168 → 30,168 B |
| Collection 1000 | 203,463.81 / 204,349.09 ns | 217,644.10 / 217,916.35 ns | 299,640 → 299,640 B |

Warm allocations and Gen0/Gen1 behavior are unchanged. Latency variation is within the
shared-host ShortRun noise envelope; the mapping path was not modified.

## Cold configuration + first Map

| Scenario | 4.2 mean | 4.3 mean | Allocated 4.2 → 4.3 |
| --- | ---: | ---: | ---: |
| Flat DTO | 515.52 ns | 547.21 ns | 2,168 → 2,216 B |
| Nested object | 917.83 ns | 977.07 ns | 3,000 → 3,080 B |
| Custom ForMember | 1,518.48 ns | 1,567.71 ns | 5,016 → 5,064 B |
| Circular reference | 484.14 ns | 480.26 ns | 1,984 → 2,032 B |
| Collection 10 | 2,524.47 ns | 2,527.83 ns | 5,016 → 5,080 B |
| Collection 100 | 19,823.85 ns | 21,135.68 ns | 31,920 → 31,984 B |
| Collection 1000 | 204,817.81 ns | 215,893.14 ns | 301,392 → 301,456 B |

Cold configuration allocates **32 bytes per configuration/profile registrar and 16 bytes per
map expression** for rollback state. This is a one-time setup cost, not a per-Map
allocation. Cold timings remain within noise.

## Interpretation

- No mapping-hot-path change and no warm allocation regression.
- Atomicity and validation add only configuration-time memory.
- Startup validation in the DI package builds the mapper earlier but does not alter the
  mapping hot path.
- The Phase 3.2 engine gains remain intact.

Raw report: `phase-4.3-after.md`.
