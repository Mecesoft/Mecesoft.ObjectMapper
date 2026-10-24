```

BenchmarkDotNet v0.15.8, macOS 27.0.1 (26A434) [Darwin 27.0.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK 10.0.102
  [Host]   : .NET 10.0.2 (10.0.2, 10.0.225.61305), Arm64 RyuJIT armv8.0-a
  ShortRun : .NET 10.0.2 (10.0.2, 10.0.225.61305), Arm64 RyuJIT armv8.0-a

Job=ShortRun  Server=False  IterationCount=3  
LaunchCount=1  WarmupCount=3  

```
| Type                        | Method            | Count | Mean          | Error         | StdDev       | Median        | Ratio  | RatioSD | Gen0    | Gen1    | Allocated | Alloc Ratio |
|---------------------------- |------------------ |------ |--------------:|--------------:|-------------:|--------------:|-------:|--------:|--------:|--------:|----------:|------------:|
| CircularMappingBenchmarks   | Manual_Circular   | ?     |      13.33 ns |      4.503 ns |     0.247 ns |      13.27 ns |   1.00 |    0.02 |  0.0102 |       - |      64 B |        1.00 |
| FlatMappingBenchmarks       | Manual_Flat       | ?     |      14.23 ns |      4.307 ns |     0.236 ns |      14.21 ns |   1.07 |    0.02 |  0.0179 |       - |     112 B |        1.75 |
| NestedMappingBenchmarks     | Manual_Nested     | ?     |      45.13 ns |      5.329 ns |     0.292 ns |      44.97 ns |   3.39 |    0.06 |  0.0561 |       - |     352 B |        5.50 |
| CustomMappingBenchmarks     | Manual_Custom     | ?     |      49.84 ns |      7.392 ns |     0.405 ns |      49.71 ns |   3.74 |    0.07 |  0.0268 |       - |     168 B |        2.62 |
| CustomMappingBenchmarks     | Warm_Custom       | ?     |     209.34 ns |    102.107 ns |     5.597 ns |     212.47 ns |  15.71 |    0.44 |  0.1044 |       - |     656 B |       10.25 |
| CircularMappingBenchmarks   | Warm_Circular     | ?     |     290.36 ns |     96.217 ns |     5.274 ns |     288.24 ns |  21.79 |    0.49 |  0.0839 |       - |     528 B |        8.25 |
| FlatMappingBenchmarks       | Warm_Flat         | ?     |     306.34 ns |     40.561 ns |     2.223 ns |     305.66 ns |  22.99 |    0.39 |  0.1135 |       - |     712 B |       11.12 |
| CircularMappingBenchmarks   | Cold_Circular     | ?     |     484.14 ns |     63.550 ns |     3.483 ns |     485.87 ns |  36.33 |    0.62 |  0.3157 |  0.0010 |    1984 B |       31.00 |
| FlatMappingBenchmarks       | Cold_Flat         | ?     |     515.52 ns |     58.048 ns |     3.182 ns |     514.02 ns |  38.69 |    0.65 |  0.3452 |  0.0010 |    2168 B |       33.88 |
| NestedMappingBenchmarks     | Warm_Nested       | ?     |     548.53 ns |     80.998 ns |     4.440 ns |     550.79 ns |  41.17 |    0.72 |  0.1516 |       - |     952 B |       14.88 |
| NestedMappingBenchmarks     | Cold_Nested       | ?     |     917.83 ns |    186.244 ns |    10.209 ns |     917.17 ns |  68.88 |    1.28 |  0.4778 |  0.0019 |    3000 B |       46.88 |
| CustomMappingBenchmarks     | Cold_Custom       | ?     |   1,518.48 ns |    459.053 ns |    25.162 ns |   1,507.20 ns | 113.96 |    2.44 |  0.7935 |       - |    5016 B |       78.38 |
|                             |                   |       |               |               |              |               |        |         |         |         |           |             |
| CollectionMappingBenchmarks | Manual_Collection | 10    |     111.02 ns |     33.958 ns |     1.861 ns |     111.40 ns |   1.00 |    0.02 |  0.1198 |  0.0002 |     752 B |        1.00 |
| CollectionMappingBenchmarks | Warm_Collection   | 10    |   2,209.97 ns |    535.165 ns |    29.334 ns |   2,204.42 ns |  19.91 |    0.37 |  0.5188 |       - |    3264 B |        4.34 |
| CollectionMappingBenchmarks | Cold_Collection   | 10    |   2,524.47 ns |    782.549 ns |    42.894 ns |   2,535.73 ns |  22.74 |    0.47 |  0.7973 |  0.0076 |    5016 B |        6.67 |
|                             |                   |       |               |               |              |               |        |         |         |         |           |             |
| CollectionMappingBenchmarks | Manual_Collection | 100   |     890.99 ns |    106.958 ns |     5.863 ns |     893.44 ns |   1.00 |    0.01 |  1.0376 |  0.0238 |    6512 B |        1.00 |
| CollectionMappingBenchmarks | Cold_Collection   | 100   |  19,823.85 ns |    571.008 ns |    31.299 ns |  19,818.78 ns |  22.25 |    0.13 |  5.0659 |  0.2747 |   31920 B |        4.90 |
| CollectionMappingBenchmarks | Warm_Collection   | 100   |  20,128.80 ns |  5,149.926 ns |   282.285 ns |  20,250.17 ns |  22.59 |    0.30 |  4.7913 |  0.2747 |   30168 B |        4.63 |
|                             |                   |       |               |               |              |               |        |         |         |         |           |             |
| CollectionMappingBenchmarks | Manual_Collection | 1000  |   8,992.35 ns |  5,462.776 ns |   299.433 ns |   8,869.83 ns |   1.00 |    0.04 | 10.2081 |  1.8005 |   64112 B |        1.00 |
| CollectionMappingBenchmarks | Warm_Collection   | 1000  | 203,463.81 ns | 31,517.913 ns | 1,727.603 ns | 204,349.09 ns |  22.64 |    0.66 | 47.6074 | 15.8691 |  299640 B |        4.67 |
| CollectionMappingBenchmarks | Cold_Collection   | 1000  | 204,817.81 ns | 70,828.311 ns | 3,882.339 ns | 204,238.42 ns |  22.79 |    0.75 | 47.6074 | 15.8691 |  301392 B |        4.70 |
