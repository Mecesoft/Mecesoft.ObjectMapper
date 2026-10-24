using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

var config = ManualConfig.CreateMinimumViable()
    .AddJob(Job.ShortRun
        .WithId("ShortRun")
        .WithGcServer(false))
    .AddDiagnoser(MemoryDiagnoser.Default)
    .AddColumn(StatisticColumn.Median)
    .AddExporter(MarkdownExporter.GitHub)
    .WithOrderer(new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest))
    .WithOptions(ConfigOptions.JoinSummary);

var argsToUse = args.Length == 0 ? new[] { "--filter", "*" } : args;
BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(argsToUse, config);
