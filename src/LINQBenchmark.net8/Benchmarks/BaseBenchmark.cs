using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using LINQBenchmark.Helpers;
using LINQBenchmark.Models;

using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace LINQBenchmark.net8.Benchmarks;

[SimpleJob(RuntimeMoniker.Net80, baseline: true)]
[MemoryDiagnoser]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
public abstract class BaseBenchmark
{
    protected const int DataSize = 100;
    protected TestData[] _testData = null!;
    
    // Метрики для сбора
    protected static readonly Meter Meter = new("LINQBenchmark");
    protected static readonly Counter<int> CollectionPassesCounter = Meter.CreateCounter<int>("collection_passes");
    protected static readonly Counter<long> MemoryAllocationsCounter = Meter.CreateCounter<long>("memory_allocations");
    protected static readonly Counter<int> AllocationsCountCounter = Meter.CreateCounter<int>("allocations_count");

    [GlobalSetup]
    public void Setup()
    {
        _testData = DataGenerator.GenerateTestData(DataSize);
    }
    
    protected void LogCollectionPasses(string method, int passes)
    {
        var logPath = Path.Combine(Directory.GetCurrentDirectory(), "collection_passes.log");
        File.AppendAllText(logPath, $"{DateTime.Now:HH:mm:ss} {method} - Collection Passes: {passes}\n");
    }
}