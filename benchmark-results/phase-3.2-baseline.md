```

BenchmarkDotNet v0.15.8, macOS 27.0.1 (26A434) [Darwin 27.0.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK 10.0.102
  [Host]   : .NET 10.0.2 (10.0.2, 10.0.225.61305), Arm64 RyuJIT armv8.0-a
  ShortRun : .NET 10.0.2 (10.0.2, 10.0.225.61305), Arm64 RyuJIT armv8.0-a

Job=ShortRun  Server=False  IterationCount=3  
LaunchCount=1  WarmupCount=3  

```
| Type                        | Method            | Count | Mean          | Error         | StdDev       | Median        | Ratio  | RatioSD | Gen0     | Gen1     | Allocated | Alloc Ratio |
|---------------------------- |------------------ |------ |--------------:|--------------:|-------------:|--------------:|-------:|--------:|---------:|---------:|----------:|------------:|
| CircularMappingBenchmarks   | Manual_Circular   | ?     |      11.42 ns |      0.618 ns |     0.034 ns |      11.41 ns |   1.00 |    0.00 |   0.0102 |        - |      64 B |        1.00 |
| FlatMappingBenchmarks       | Manual_Flat       | ?     |      12.82 ns |      0.272 ns |     0.015 ns |      12.82 ns |   1.12 |    0.00 |   0.0179 |        - |     112 B |        1.75 |
| NestedMappingBenchmarks     | Manual_Nested     | ?     |      42.70 ns |      7.783 ns |     0.427 ns |      42.50 ns |   3.74 |    0.03 |   0.0561 |        - |     352 B |        5.50 |
| CustomMappingBenchmarks     | Manual_Custom     | ?     |      47.33 ns |      1.585 ns |     0.087 ns |      47.31 ns |   4.14 |    0.01 |   0.0268 |        - |     168 B |        2.62 |
| CustomMappingBenchmarks     | Warm_Custom       | ?     |     345.96 ns |     10.722 ns |     0.588 ns |     346.26 ns |  30.29 |    0.09 |   0.1235 |        - |     776 B |       12.12 |
| CircularMappingBenchmarks   | Warm_Circular     | ?     |     653.73 ns |    183.880 ns |    10.079 ns |     648.27 ns |  57.23 |    0.78 |   0.2851 |        - |    1792 B |       28.00 |
| CircularMappingBenchmarks   | Cold_Circular     | ?     |     660.94 ns |    141.505 ns |     7.756 ns |     657.03 ns |  57.86 |    0.61 |   0.3338 |   0.0010 |    2096 B |       32.75 |
| CustomMappingBenchmarks     | Cold_Custom       | ?     |   1,210.37 ns |     40.954 ns |     2.245 ns |   1,210.80 ns | 105.96 |    0.32 |   0.5417 |        - |    3400 B |       53.12 |
| FlatMappingBenchmarks       | Warm_Flat         | ?     |   1,711.70 ns |     26.206 ns |     1.436 ns |   1,711.84 ns | 149.85 |    0.40 |   0.4921 |        - |    3096 B |       48.38 |
| FlatMappingBenchmarks       | Cold_Flat         | ?     |   1,755.83 ns |     58.549 ns |     3.209 ns |   1,757.27 ns | 153.72 |    0.46 |   0.5417 |   0.0019 |    3400 B |       53.12 |
| NestedMappingBenchmarks     | Warm_Nested       | ?     |   2,614.75 ns |     82.186 ns |     4.505 ns |   2,613.24 ns | 228.91 |    0.68 |   0.9155 |   0.0038 |    5753 B |       89.89 |
| NestedMappingBenchmarks     | Cold_Nested       | ?     |   2,696.77 ns |    337.191 ns |    18.483 ns |   2,703.60 ns | 236.09 |    1.53 |   1.0185 |   0.0076 |    6393 B |       99.89 |
|                             |                   |       |               |               |              |               |        |         |          |          |           |             |
| CollectionMappingBenchmarks | Manual_Collection | 10    |      99.95 ns |      1.660 ns |     0.091 ns |      99.90 ns |   1.00 |    0.00 |   0.1198 |   0.0002 |     752 B |        1.00 |
| CollectionMappingBenchmarks | Warm_Collection   | 10    |   6,365.43 ns |  1,748.647 ns |    95.849 ns |   6,314.42 ns |  63.69 |    0.83 |   2.7466 |        - |   17242 B |       22.93 |
| CollectionMappingBenchmarks | Cold_Collection   | 10    |   7,342.26 ns | 22,218.019 ns | 1,217.845 ns |   6,846.48 ns |  73.46 |   10.55 |   2.8076 |        - |   17714 B |       23.56 |
|                             |                   |       |               |               |              |               |        |         |          |          |           |             |
| CollectionMappingBenchmarks | Manual_Collection | 100   |     800.48 ns |     68.813 ns |     3.772 ns |     799.80 ns |   1.00 |    0.01 |   1.0376 |   0.0238 |    6512 B |        1.00 |
| CollectionMappingBenchmarks | Warm_Collection   | 100   |  57,919.27 ns |  3,495.026 ns |   191.574 ns |  57,862.73 ns |  72.36 |    0.36 |  25.3906 |   2.9297 |  159854 B |       24.55 |
| CollectionMappingBenchmarks | Cold_Collection   | 100   |  59,549.51 ns |  9,241.297 ns |   506.547 ns |  59,641.05 ns |  74.39 |    0.63 |  25.3906 |   2.9297 |  160326 B |       24.62 |
|                             |                   |       |               |               |              |               |        |         |          |          |           |             |
| CollectionMappingBenchmarks | Manual_Collection | 1000  |   8,003.29 ns |  2,394.506 ns |   131.251 ns |   7,932.02 ns |   1.00 |    0.02 |  10.2081 |   1.8005 |   64112 B |        1.00 |
| CollectionMappingBenchmarks | Warm_Collection   | 1000  | 619,695.21 ns | 65,193.068 ns | 3,573.452 ns | 619,339.36 ns |  77.44 |    1.16 | 250.0000 | 125.0000 | 1592156 B |       24.83 |
| CollectionMappingBenchmarks | Cold_Collection   | 1000  | 627,583.01 ns | 76,689.277 ns | 4,203.598 ns | 629,018.88 ns |  78.43 |    1.19 | 253.9063 | 125.0000 | 1592745 B |       24.84 |
