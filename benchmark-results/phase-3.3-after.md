```

BenchmarkDotNet v0.15.8, macOS 27.0.1 (26A434) [Darwin 27.0.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK 10.0.102
  [Host]   : .NET 10.0.2 (10.0.2, 10.0.225.61305), Arm64 RyuJIT armv8.0-a
  ShortRun : .NET 10.0.2 (10.0.2, 10.0.225.61305), Arm64 RyuJIT armv8.0-a

Job=ShortRun  Server=False  IterationCount=3  
LaunchCount=1  WarmupCount=3  

```
| Type                        | Method            | Count | Mean          | Error          | StdDev       | Median        | Ratio  | RatioSD | Gen0    | Gen1    | Allocated | Alloc Ratio |
|---------------------------- |------------------ |------ |--------------:|---------------:|-------------:|--------------:|-------:|--------:|--------:|--------:|----------:|------------:|
| FlatMappingBenchmarks       | Manual_Flat       | ?     |      12.99 ns |       4.893 ns |     0.268 ns |      12.92 ns |   1.00 |    0.03 |  0.0179 |       - |     112 B |        1.00 |
| CircularMappingBenchmarks   | Manual_Circular   | ?     |      16.34 ns |      74.542 ns |     4.086 ns |      16.36 ns |   1.26 |    0.27 |  0.0102 |       - |      64 B |        0.57 |
| NestedMappingBenchmarks     | Manual_Nested     | ?     |      43.82 ns |      12.169 ns |     0.667 ns |      43.81 ns |   3.38 |    0.07 |  0.0561 |       - |     352 B |        3.14 |
| CustomMappingBenchmarks     | Manual_Custom     | ?     |      48.09 ns |      21.908 ns |     1.201 ns |      48.52 ns |   3.70 |    0.10 |  0.0268 |       - |     168 B |        1.50 |
| CustomMappingBenchmarks     | Warm_Custom       | ?     |     184.45 ns |      67.063 ns |     3.676 ns |     184.42 ns |  14.21 |    0.35 |  0.1032 |       - |     648 B |        5.79 |
| CircularMappingBenchmarks   | Warm_Circular     | ?     |     261.09 ns |      50.544 ns |     2.770 ns |     259.52 ns |  20.11 |    0.40 |  0.0825 |       - |     520 B |        4.64 |
| FlatMappingBenchmarks       | Warm_Flat         | ?     |     287.46 ns |     116.994 ns |     6.413 ns |     290.34 ns |  22.14 |    0.58 |  0.1121 |       - |     704 B |        6.29 |
| FlatMappingBenchmarks       | Cold_Flat         | ?     |     472.17 ns |     134.899 ns |     7.394 ns |     469.15 ns |  36.37 |    0.81 |  0.3390 |  0.0014 |    2128 B |       19.00 |
| NestedMappingBenchmarks     | Warm_Nested       | ?     |     492.99 ns |      48.746 ns |     2.672 ns |     491.74 ns |  37.97 |    0.70 |  0.1497 |       - |     944 B |        8.43 |
| CircularMappingBenchmarks   | Cold_Circular     | ?     |     503.60 ns |     529.278 ns |    29.012 ns |     514.84 ns |  38.79 |    2.05 |  0.3090 |  0.0019 |    1944 B |       17.36 |
| NestedMappingBenchmarks     | Cold_Nested       | ?     |     830.00 ns |      89.160 ns |     4.887 ns |     830.60 ns |  63.93 |    1.18 |  0.4692 |  0.0019 |    2944 B |       26.29 |
| CustomMappingBenchmarks     | Cold_Custom       | ?     |   1,359.89 ns |     211.239 ns |    11.579 ns |   1,359.86 ns | 104.75 |    2.01 |  0.7858 |       - |    4976 B |       44.43 |
|                             |                   |       |               |                |              |               |        |         |         |         |           |             |
| CollectionMappingBenchmarks | Manual_Collection | 10    |     115.95 ns |      68.227 ns |     3.740 ns |     114.25 ns |   1.00 |    0.04 |  0.1197 |  0.0002 |     752 B |        1.00 |
| CollectionMappingBenchmarks | Warm_Collection   | 10    |   2,178.31 ns |   1,553.574 ns |    85.157 ns |   2,181.01 ns |  18.80 |    0.82 |  0.5188 |       - |    3256 B |        4.33 |
| CollectionMappingBenchmarks | Cold_Collection   | 10    |   2,387.27 ns |     888.898 ns |    48.723 ns |   2,401.21 ns |  20.60 |    0.67 |  0.7896 |  0.0038 |    4968 B |        6.61 |
|                             |                   |       |               |                |              |               |        |         |         |         |           |             |
| CollectionMappingBenchmarks | Manual_Collection | 100   |     921.06 ns |   1,136.280 ns |    62.283 ns |     890.23 ns |   1.00 |    0.08 |  1.0376 |  0.0238 |    6512 B |        1.00 |
| CollectionMappingBenchmarks | Warm_Collection   | 100   |  19,673.40 ns |  11,954.409 ns |   655.262 ns |  19,811.75 ns |  21.42 |    1.36 |  4.7913 |  0.2441 |   30160 B |        4.63 |
| CollectionMappingBenchmarks | Cold_Collection   | 100   |  20,053.78 ns |  13,220.724 ns |   724.673 ns |  20,040.33 ns |  21.84 |    1.41 |  5.0659 |  0.2441 |   31872 B |        4.89 |
|                             |                   |       |               |                |              |               |        |         |         |         |           |             |
| CollectionMappingBenchmarks | Manual_Collection | 1000  |   8,761.61 ns |   4,061.235 ns |   222.610 ns |   8,750.54 ns |   1.00 |    0.03 | 10.2081 |  1.8005 |   64112 B |        1.00 |
| CollectionMappingBenchmarks | Cold_Collection   | 1000  | 189,549.19 ns |  20,977.776 ns | 1,149.863 ns | 189,179.90 ns |  21.64 |    0.49 | 47.6074 | 15.8691 |  301344 B |        4.70 |
| CollectionMappingBenchmarks | Warm_Collection   | 1000  | 198,026.60 ns | 179,146.133 ns | 9,819.604 ns | 203,538.81 ns |  22.61 |    1.09 | 47.6074 | 15.8691 |  299632 B |        4.67 |
