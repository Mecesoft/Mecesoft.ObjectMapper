# Phase 3.5 vs Phase 4.2 performance

Environment: Apple M1, 8 logical cores, macOS 27.0.1, .NET SDK 10.0.102,
.NET 10.0.2 Arm64 RyuJIT, BenchmarkDotNet 0.15.8, `ShortRun`
(3 warmup / 3 iterations, Server GC disabled), benchmark target `net10.0`.

Phase 4.2 changes configuration-time code only: `ReverseMap` registration and an internal
registrar reference. The mapping hot path is unchanged. All benchmark collections are below
the configured collection limit.

## Warm-cache

| Scenario | 3.5 mean / median | 4.2 mean / median | Allocated 3.5 → 4.2 | Normalized 3.5 → 4.2 |
| --- | ---: | ---: | ---: | ---: |
| Flat DTO | 323.53 / 323.45 ns | 306.34 / 305.66 ns | 712 → 712 B | ~same |
| Nested object | 610.41 / 611.99 ns | 548.53 / 550.79 ns | 952 → 952 B | ~same |
| Custom ForMember | 216.59 / 213.01 ns | 209.34 / 212.47 ns | 656 → 656 B | within noise |
| Circular reference | 250.53 / 250.44 ns | 290.36 / 288.24 ns | 528 → 528 B | within noise |
| Collection 10 | 2,151.31 / 2,148.86 ns | 2,209.97 / 2,204.42 ns | 3,264 → 3,264 B | within noise |
| Collection 100 | 19,610.43 / 19,515.14 ns | 20,128.80 / 20,250.17 ns | 30,168 → 30,168 B | within noise |
| Collection 1000 | 201,003.67 / 201,601.58 ns | 203,463.81 / 204,349.09 ns | 299,640 → 299,640 B | within noise |

Warm allocations and Gen0/Gen1 behavior are unchanged. Variation is within the shared-host
ShortRun noise envelope.

## Cold configuration + first Map

| Scenario | 3.5 mean | 4.2 mean | Allocated 3.5 → 4.2 |
| --- | ---: | ---: | ---: |
| Flat DTO | 535.50 ns | 515.52 ns | 2,160 → 2,168 B |
| Nested object | 951.64 ns | 917.83 ns | 2,976 → 3,000 B |
| Custom ForMember | 1,972.70 ns | 1,518.48 ns | 5,088 → 5,016 B |
| Circular reference | 433.75 ns | 484.14 ns | 1,976 → 1,984 B |
| Collection 10 | 3,178.21 ns | 2,524.47 ns | 5,000 → 5,016 B |
| Collection 100 | 19,989.44 ns | 19,823.85 ns | 31,904 → 31,920 B |
| Collection 1000 | 199,788.73 ns | 204,817.81 ns | 301,376 → 301,392 B |

Cold allocations are essentially unchanged (single-digit to low-tens of bytes per
configuration) after replacing per-expression delegates with an internal registrar
reference. Cold timing remains dominated by configuration creation and host noise.

## Interpretation

- No mapping-hot-path change and no warm allocation regression.
- ReverseMap is configuration-time only; it does not add a mapping engine branch.
- Relative to the engine optimization baseline (Phase 3.2), the warm mapping and collection
  gains reported in earlier phases remain intact.

Raw reports: `phase-3.5-after.md`, `phase-4.2-after.md`.
