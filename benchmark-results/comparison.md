# Mapping Performance: Phase 3.2 baseline vs Phase 3.3

## Environment

- Hardware: Apple M1, 8 logical cores
- OS: macOS 27.0.1 (Darwin 27.0.0)
- SDK: .NET SDK 10.0.102
- Runtime: .NET 10.0.2, Arm64 RyuJIT
- BenchmarkDotNet 0.15.8
- Job: `ShortRun`, Server GC disabled, 3 warmup / 3 iterations / 1 launch
- Benchmark target: `net10.0`

Both runs used the same benchmark project, job settings, hardware and runtime. The
machine was shared, so the in-run manual mapping baseline is included for normalization.
In the final run the manual baselines were close to the baseline run.

`Cold` means a new `MapperConfiguration` + `CreateMapper` + first `Map` per invocation.
JIT warmup is handled by BenchmarkDotNet, so this is cold configuration/plan construction,
not cold JIT.

## Warm-cache results

| Scenario | Count | Baseline mean | Baseline median | After mean | After median | Speedup | Baseline allocated | After allocated | Allocation change | Baseline Gen0/Gen1 | After Gen0/Gen1 |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| Flat DTO | - | 1,711.70 ns | 1,711.84 ns | 287.46 ns | 290.34 ns | 5.95x | 3,096 B | 704 B | -77.3% | 0.4921 / 0 | 0.1121 / 0 |
| Nested object | - | 2,614.75 ns | 2,613.24 ns | 492.99 ns | 491.74 ns | 5.30x | 5,753 B | 944 B | -83.6% | 0.9155 / 0.0038 | 0.1497 / 0 |
| Custom ForMember/MapFrom | - | 345.96 ns | 346.26 ns | 184.45 ns | 184.42 ns | 1.88x | 776 B | 648 B | -16.5% | 0.1235 / 0 | 0.1032 / 0 |
| Circular reference | - | 653.73 ns | 648.27 ns | 261.09 ns | 259.52 ns | 2.50x | 1,792 B | 520 B | -71.0% | 0.2851 / 0 | 0.0825 / 0 |
| Collection 10 | 10 | 6,365.43 ns | 6,314.42 ns | 2,178.31 ns | 2,181.01 ns | 2.92x | 17,242 B | 3,256 B | -81.1% | 2.7466 / 0 | 0.5188 / 0 |
| Collection 100 | 100 | 57,919.27 ns | 57,862.73 ns | 19,673.40 ns | 19,811.75 ns | 2.94x | 159,854 B | 30,160 B | -81.1% | 25.3906 / 2.9297 | 4.7913 / 0.2441 |
| Collection 1000 | 1000 | 619,695.21 ns | 619,339.36 ns | 198,026.60 ns | 203,538.81 ns | 3.13x | 1,592,156 B | 299,632 B | -81.2% | 250 / 125 | 47.6074 / 15.8691 |

Gen0/Gen1 are collections per 1,000 operations.

Normalized to the in-run manual baseline, the warm-cache improvement is:

| Scenario | Baseline mapping/manual ratio | After mapping/manual ratio | Normalized speedup |
| --- | ---: | ---: | ---: |
| Flat DTO | 133.5x | 22.1x | 6.03x |
| Nested object | 61.2x | 11.3x | 5.44x |
| Custom ForMember/MapFrom | 7.31x | 3.84x | 1.90x |
| Circular reference | 57.2x | 16.0x | 3.58x |
| Collection 10 | 63.7x | 18.8x | 3.39x |
| Collection 100 | 72.4x | 21.4x | 3.39x |
| Collection 1000 | 77.4x | 22.6x | 3.43x |

## Cold-start results

| Scenario | Count | Baseline mean | Baseline median | After mean | After median | Speedup | Baseline allocated | After allocated | Allocation change | Baseline Gen0/Gen1 | After Gen0/Gen1 |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| Flat DTO | - | 1,755.83 ns | 1,757.27 ns | 472.17 ns | 469.15 ns | 3.72x | 3,400 B | 2,128 B | -37.4% | 0.5417 / 0.0019 | 0.3390 / 0.0014 |
| Nested object | - | 2,696.77 ns | 2,703.60 ns | 830.00 ns | 830.60 ns | 3.25x | 6,393 B | 2,944 B | -53.9% | 1.0185 / 0.0076 | 0.4692 / 0.0019 |
| Custom ForMember/MapFrom | - | 1,210.37 ns | 1,210.80 ns | 1,359.89 ns | 1,359.86 ns | 0.89x | 3,400 B | 4,976 B | +46.4% | 0.5417 / 0 | 0.7858 / 0 |
| Circular reference | - | 660.94 ns | 657.03 ns | 503.60 ns | 514.84 ns | 1.31x | 2,096 B | 1,944 B | -7.3% | 0.3338 / 0.0010 | 0.3090 / 0.0019 |
| Collection 10 | 10 | 7,342.26 ns | 6,846.48 ns | 2,387.27 ns | 2,401.21 ns | 3.08x | 17,714 B | 4,968 B | -72.0% | 2.8076 / 0 | 0.7896 / 0.0038 |
| Collection 100 | 100 | 59,549.51 ns | 59,641.05 ns | 20,053.78 ns | 20,040.33 ns | 2.97x | 160,326 B | 31,872 B | -80.1% | 25.3906 / 2.9297 | 5.0659 / 0.2441 |
| Collection 1000 | 1000 | 627,583.01 ns | 629,018.88 ns | 189,549.19 ns | 189,179.90 ns | 3.31x | 1,592,745 B | 301,344 B | -81.1% | 253.9063 / 125 | 47.6074 / 15.8691 |

Cold custom mapping is the only regression: `ForMember` plans are configuration-scoped
and include distinct delegates, so they are rebuilt for each new mapper. The absolute
difference is about 0.15 microseconds per new mapper. Conventional maps share plans
process-wide, which is why cold flat/nested/collection cases improved.

## Takeaways

- Warm mapping is roughly 1.9x–6.0x faster depending on scenario.
- Collection allocation dropped by about 81% at every tested size.
- Large collection Gen1 collections per 1,000 operations dropped from 125 to 15.9
  for 1,000 elements.
- Cold configuration + first map is faster for conventional maps and collections.
- The largest remaining allocation costs are destination graph allocation, reference
  tracking entries, boxing of value-type members/elements, and collection enumeration.

Raw BenchmarkDotNet reports are stored alongside this file:

- `phase-3.2-baseline.md`
- `phase-3.3-after.md`
