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
| CircularMappingBenchmarks   | Manual_Circular   | ?     |      12.84 ns |       1.584 ns |     0.087 ns |      12.85 ns |   1.00 |    0.01 |  0.0102 |       - |      64 B |        1.00 |
| FlatMappingBenchmarks       | Manual_Flat       | ?     |      15.25 ns |      12.657 ns |     0.694 ns |      14.91 ns |   1.19 |    0.05 |  0.0178 |       - |     112 B |        1.75 |
| NestedMappingBenchmarks     | Manual_Nested     | ?     |      51.74 ns |      36.200 ns |     1.984 ns |      52.22 ns |   4.03 |    0.14 |  0.0561 |       - |     352 B |        5.50 |
| CustomMappingBenchmarks     | Manual_Custom     | ?     |      54.83 ns |       3.574 ns |     0.196 ns |      54.80 ns |   4.27 |    0.03 |  0.0268 |       - |     168 B |        2.62 |
| CustomMappingBenchmarks     | Warm_Custom       | ?     |     224.79 ns |      61.693 ns |     3.382 ns |     225.95 ns |  17.51 |    0.25 |  0.1044 |       - |     656 B |       10.25 |
| FlatMappingBenchmarks       | Warm_Flat         | ?     |     322.16 ns |     123.653 ns |     6.778 ns |     325.28 ns |  25.09 |    0.48 |  0.1135 |       - |     712 B |       11.12 |
| CircularMappingBenchmarks   | Warm_Circular     | ?     |     331.40 ns |   1,561.457 ns |    85.589 ns |     285.50 ns |  25.81 |    5.78 |  0.0839 |       - |     528 B |        8.25 |
| CircularMappingBenchmarks   | Cold_Circular     | ?     |     497.19 ns |     254.674 ns |    13.960 ns |     490.89 ns |  38.73 |    0.97 |  0.3147 |  0.0010 |    1976 B |       30.88 |
| FlatMappingBenchmarks       | Cold_Flat         | ?     |     553.62 ns |      56.157 ns |     3.078 ns |     553.73 ns |  43.12 |    0.33 |  0.3443 |  0.0010 |    2160 B |       33.75 |
| NestedMappingBenchmarks     | Warm_Nested       | ?     |     621.65 ns |     217.735 ns |    11.935 ns |     616.57 ns |  48.42 |    0.85 |  0.1516 |       - |     952 B |       14.88 |
| NestedMappingBenchmarks     | Cold_Nested       | ?     |     956.62 ns |     105.781 ns |     5.798 ns |     959.44 ns |  74.51 |    0.59 |  0.4740 |  0.0029 |    2976 B |       46.50 |
| CustomMappingBenchmarks     | Cold_Custom       | ?     |   1,681.30 ns |   2,077.698 ns |   113.886 ns |   1,642.97 ns | 130.96 |    7.72 |  0.7935 |       - |    5008 B |       78.25 |
|                             |                   |       |               |                |              |               |        |         |         |         |           |             |
| CollectionMappingBenchmarks | Manual_Collection | 10    |     116.45 ns |      21.873 ns |     1.199 ns |     116.75 ns |   1.00 |    0.01 |  0.1197 |  0.0002 |     752 B |        1.00 |
| CollectionMappingBenchmarks | Warm_Collection   | 10    |   2,412.82 ns |      30.467 ns |     1.670 ns |   2,413.39 ns |  20.72 |    0.19 |  0.5188 |       - |    3264 B |        4.34 |
| CollectionMappingBenchmarks | Cold_Collection   | 10    |   2,663.55 ns |     687.686 ns |    37.694 ns |   2,660.49 ns |  22.87 |    0.35 |  0.7935 |  0.0076 |    5000 B |        6.65 |
|                             |                   |       |               |                |              |               |        |         |         |         |           |             |
| CollectionMappingBenchmarks | Manual_Collection | 100   |     945.22 ns |     271.862 ns |    14.902 ns |     936.78 ns |   1.00 |    0.02 |  1.0376 |  0.0238 |    6512 B |        1.00 |
| CollectionMappingBenchmarks | Warm_Collection   | 100   |  21,380.18 ns |   2,748.125 ns |   150.634 ns |  21,326.17 ns |  22.62 |    0.34 |  4.7913 |  0.2747 |   30168 B |        4.63 |
| CollectionMappingBenchmarks | Cold_Collection   | 100   |  22,109.36 ns |     945.304 ns |    51.815 ns |  22,111.13 ns |  23.39 |    0.32 |  5.0659 |  0.2747 |   31904 B |        4.90 |
|                             |                   |       |               |                |              |               |        |         |         |         |           |             |
| CollectionMappingBenchmarks | Manual_Collection | 1000  |   9,677.79 ns |   2,428.537 ns |   133.116 ns |   9,733.76 ns |   1.00 |    0.02 | 10.2081 |  1.8005 |   64112 B |        1.00 |
| CollectionMappingBenchmarks | Warm_Collection   | 1000  | 220,659.90 ns |  38,096.490 ns | 2,088.197 ns | 220,662.67 ns |  22.80 |    0.33 | 47.6074 | 15.8691 |  299640 B |        4.67 |
| CollectionMappingBenchmarks | Cold_Collection   | 1000  | 224,339.05 ns | 118,040.202 ns | 6,470.182 ns | 226,547.09 ns |  23.18 |    0.64 | 47.6074 | 15.8691 |  301376 B |        4.70 |
