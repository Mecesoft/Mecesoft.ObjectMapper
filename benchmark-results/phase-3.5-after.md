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
| FlatMappingBenchmarks       | Manual_Flat       | ?     |      14.95 ns |      6.099 ns |     0.334 ns |      15.01 ns |   1.00 |    0.03 |  0.0179 |       - |     112 B |        1.00 |
| CircularMappingBenchmarks   | Manual_Circular   | ?     |      15.25 ns |     76.702 ns |     4.204 ns |      12.99 ns |   1.02 |    0.24 |  0.0102 |       - |      64 B |        0.57 |
| NestedMappingBenchmarks     | Manual_Nested     | ?     |      49.56 ns |     24.216 ns |     1.327 ns |      48.99 ns |   3.32 |    0.10 |  0.0561 |       - |     352 B |        3.14 |
| CustomMappingBenchmarks     | Manual_Custom     | ?     |      55.50 ns |     72.265 ns |     3.961 ns |      56.50 ns |   3.71 |    0.24 |  0.0268 |       - |     168 B |        1.50 |
| CustomMappingBenchmarks     | Warm_Custom       | ?     |     216.59 ns |    216.950 ns |    11.892 ns |     213.01 ns |  14.50 |    0.75 |  0.1044 |       - |     656 B |        5.86 |
| CircularMappingBenchmarks   | Warm_Circular     | ?     |     250.53 ns |     13.352 ns |     0.732 ns |     250.44 ns |  16.77 |    0.33 |  0.0839 |       - |     528 B |        4.71 |
| FlatMappingBenchmarks       | Warm_Flat         | ?     |     323.53 ns |    167.337 ns |     9.172 ns |     323.45 ns |  21.65 |    0.68 |  0.1135 |       - |     712 B |        6.36 |
| CircularMappingBenchmarks   | Cold_Circular     | ?     |     433.75 ns |     46.685 ns |     2.559 ns |     433.91 ns |  29.03 |    0.58 |  0.3147 |  0.0010 |    1976 B |       17.64 |
| FlatMappingBenchmarks       | Cold_Flat         | ?     |     535.50 ns |    117.173 ns |     6.423 ns |     538.64 ns |  35.84 |    0.79 |  0.3443 |  0.0010 |    2160 B |       19.29 |
| NestedMappingBenchmarks     | Warm_Nested       | ?     |     610.41 ns |    215.180 ns |    11.795 ns |     611.99 ns |  40.85 |    1.05 |  0.1516 |       - |     952 B |        8.50 |
| NestedMappingBenchmarks     | Cold_Nested       | ?     |     951.64 ns |    260.704 ns |    14.290 ns |     946.60 ns |  63.69 |    1.49 |  0.4740 |  0.0029 |    2976 B |       26.57 |
| CustomMappingBenchmarks     | Cold_Custom       | ?     |   1,972.70 ns |  3,533.858 ns |   193.703 ns |   1,929.68 ns | 132.03 |   11.52 |  0.8087 |       - |    5088 B |       45.43 |
|                             |                   |       |               |               |              |               |        |         |         |         |           |             |
| CollectionMappingBenchmarks | Manual_Collection | 10    |     100.17 ns |     13.929 ns |     0.764 ns |      99.95 ns |   1.00 |    0.01 |  0.1198 |  0.0002 |     752 B |        1.00 |
| CollectionMappingBenchmarks | Warm_Collection   | 10    |   2,151.31 ns |  1,019.296 ns |    55.871 ns |   2,148.86 ns |  21.48 |    0.50 |  0.5188 |       - |    3264 B |        4.34 |
| CollectionMappingBenchmarks | Cold_Collection   | 10    |   3,178.21 ns |  5,801.870 ns |   318.020 ns |   3,325.44 ns |  31.73 |    2.76 |  0.7935 |  0.0076 |    5000 B |        6.65 |
|                             |                   |       |               |               |              |               |        |         |         |         |           |             |
| CollectionMappingBenchmarks | Manual_Collection | 100   |     826.94 ns |    229.647 ns |    12.588 ns |     830.77 ns |   1.00 |    0.02 |  1.0376 |  0.0238 |    6512 B |        1.00 |
| CollectionMappingBenchmarks | Warm_Collection   | 100   |  19,610.43 ns |  4,067.566 ns |   222.957 ns |  19,515.14 ns |  23.72 |    0.39 |  4.7913 |  0.2747 |   30168 B |        4.63 |
| CollectionMappingBenchmarks | Cold_Collection   | 100   |  19,989.44 ns |  3,366.519 ns |   184.530 ns |  19,930.97 ns |  24.18 |    0.37 |  5.0659 |  0.2747 |   31904 B |        4.90 |
|                             |                   |       |               |               |              |               |        |         |         |         |           |             |
| CollectionMappingBenchmarks | Manual_Collection | 1000  |   8,492.88 ns |  3,587.195 ns |   196.626 ns |   8,602.44 ns |   1.00 |    0.03 | 10.2081 |  1.8005 |   64112 B |        1.00 |
| CollectionMappingBenchmarks | Cold_Collection   | 1000  | 199,788.73 ns | 52,277.490 ns | 2,865.506 ns | 199,094.90 ns |  23.53 |    0.56 | 47.6074 | 15.8691 |  301376 B |        4.70 |
| CollectionMappingBenchmarks | Warm_Collection   | 1000  | 201,003.67 ns | 50,936.714 ns | 2,792.013 ns | 201,601.58 ns |  23.68 |    0.56 | 47.6074 | 15.8691 |  299640 B |        4.67 |
