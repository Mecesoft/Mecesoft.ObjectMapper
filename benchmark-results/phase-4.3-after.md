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
| CircularMappingBenchmarks   | Manual_Circular   | ?     |      12.21 ns |      2.563 ns |     0.140 ns |      12.14 ns |   1.00 |    0.01 |  0.0102 |       - |      64 B |        1.00 |
| FlatMappingBenchmarks       | Manual_Flat       | ?     |      14.33 ns |      5.087 ns |     0.279 ns |      14.25 ns |   1.17 |    0.02 |  0.0179 |       - |     112 B |        1.75 |
| NestedMappingBenchmarks     | Manual_Nested     | ?     |      50.19 ns |     45.824 ns |     2.512 ns |      49.04 ns |   4.11 |    0.18 |  0.0561 |       - |     352 B |        5.50 |
| CustomMappingBenchmarks     | Manual_Custom     | ?     |      52.55 ns |      3.599 ns |     0.197 ns |      52.61 ns |   4.31 |    0.04 |  0.0268 |       - |     168 B |        2.62 |
| CustomMappingBenchmarks     | Warm_Custom       | ?     |     219.37 ns |    153.341 ns |     8.405 ns |     219.07 ns |  17.97 |    0.62 |  0.1044 |       - |     656 B |       10.25 |
| CircularMappingBenchmarks   | Warm_Circular     | ?     |     275.68 ns |    235.629 ns |    12.916 ns |     270.16 ns |  22.59 |    0.94 |  0.0839 |       - |     528 B |        8.25 |
| FlatMappingBenchmarks       | Warm_Flat         | ?     |     314.78 ns |     99.581 ns |     5.458 ns |     315.84 ns |  25.79 |    0.46 |  0.1135 |       - |     712 B |       11.12 |
| CircularMappingBenchmarks   | Cold_Circular     | ?     |     480.26 ns |    116.593 ns |     6.391 ns |     479.85 ns |  39.35 |    0.60 |  0.3233 |  0.0010 |    2032 B |       31.75 |
| FlatMappingBenchmarks       | Cold_Flat         | ?     |     547.21 ns |    128.977 ns |     7.070 ns |     547.37 ns |  44.84 |    0.67 |  0.3529 |  0.0010 |    2216 B |       34.62 |
| NestedMappingBenchmarks     | Warm_Nested       | ?     |     597.28 ns |    186.857 ns |    10.242 ns |     600.82 ns |  48.94 |    0.87 |  0.1516 |       - |     952 B |       14.88 |
| NestedMappingBenchmarks     | Cold_Nested       | ?     |     977.07 ns |     75.262 ns |     4.125 ns |     977.35 ns |  80.06 |    0.85 |  0.4902 |  0.0019 |    3080 B |       48.12 |
| CustomMappingBenchmarks     | Cold_Custom       | ?     |   1,567.71 ns |    370.400 ns |    20.303 ns |   1,572.03 ns | 128.45 |    1.92 |  0.8011 |       - |    5064 B |       79.12 |
|                             |                   |       |               |               |              |               |        |         |         |         |           |             |
| CollectionMappingBenchmarks | Manual_Collection | 10    |     110.79 ns |     25.291 ns |     1.386 ns |     111.19 ns |   1.00 |    0.02 |  0.1198 |  0.0002 |     752 B |        1.00 |
| CollectionMappingBenchmarks | Warm_Collection   | 10    |   2,190.28 ns |    313.197 ns |    17.167 ns |   2,185.12 ns |  19.77 |    0.25 |  0.5188 |       - |    3264 B |        4.34 |
| CollectionMappingBenchmarks | Cold_Collection   | 10    |   2,527.83 ns |    897.490 ns |    49.194 ns |   2,531.67 ns |  22.82 |    0.46 |  0.8087 |  0.0076 |    5080 B |        6.76 |
|                             |                   |       |               |               |              |               |        |         |         |         |           |             |
| CollectionMappingBenchmarks | Manual_Collection | 100   |     917.43 ns |    289.100 ns |    15.847 ns |     909.65 ns |   1.00 |    0.02 |  1.0376 |  0.0238 |    6512 B |        1.00 |
| CollectionMappingBenchmarks | Warm_Collection   | 100   |  20,863.57 ns |  7,500.874 ns |   411.148 ns |  20,808.73 ns |  22.75 |    0.51 |  4.7913 |  0.2747 |   30168 B |        4.63 |
| CollectionMappingBenchmarks | Cold_Collection   | 100   |  21,135.68 ns |  4,659.346 ns |   255.394 ns |  21,031.61 ns |  23.04 |    0.42 |  5.0964 |  0.2747 |   31984 B |        4.91 |
|                             |                   |       |               |               |              |               |        |         |         |         |           |             |
| CollectionMappingBenchmarks | Manual_Collection | 1000  |   9,226.85 ns |  2,453.152 ns |   134.466 ns |   9,303.82 ns |   1.00 |    0.02 | 10.2081 |  1.8005 |   64112 B |        1.00 |
| CollectionMappingBenchmarks | Cold_Collection   | 1000  | 215,893.14 ns | 75,648.111 ns | 4,146.528 ns | 214,190.87 ns |  23.40 |    0.49 | 47.6074 | 15.8691 |  301456 B |        4.70 |
| CollectionMappingBenchmarks | Warm_Collection   | 1000  | 217,644.10 ns | 15,650.125 ns |   857.836 ns | 217,916.35 ns |  23.59 |    0.31 | 47.6074 | 15.8691 |  299640 B |        4.67 |
