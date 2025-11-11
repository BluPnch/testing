using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using LINQBenchmark.Helpers;
using LINQBenchmark.Models;
using System.Diagnostics.Metrics;

namespace LINQBenchmark.net8.Benchmarks;

[SimpleJob(RuntimeMoniker.Net80, baseline: true)]
[MemoryDiagnoser]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
[IterationCount(3)]           // 3 итерации вместо ~15
[WarmupCount(1)]              // 1 разогрев вместо 10-20  
[InvocationCount(100)]        // 100 вызовов вместо 1000+
public abstract class BaseBenchmark
{
    protected const int DataSize = 100;
    protected TestData[] _testData = null!;
    
    // Метрики для Prometheus
    protected static readonly Meter Meter = new("LINQBenchmark");
    protected static readonly Counter<int> CollectionPassesCounter = Meter.CreateCounter<int>("linq_collection_passes");
    protected static readonly Counter<long> MemoryAllocationsCounter = Meter.CreateCounter<long>("linq_memory_allocated_bytes");
    protected static readonly Counter<int> AllocationsCountCounter = Meter.CreateCounter<int>("linq_allocations_count");

    [GlobalSetup]
    public void Setup()
    {
        _testData = DataGenerator.GenerateTestData(DataSize);
    }

    protected void LogCollectionPasses(string method, int passes, string runtime)
    {
        try
        {
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "collection_passes.log");
            var logMessage = $"{DateTime.Now:HH:mm:ss} {runtime} {method} - Collection Passes: {passes}\n";
        
            Console.WriteLine($"=== COLLECTION PASSES === {logMessage}"); // Явно выделяем в консоли
            File.AppendAllText(logPath, logMessage);
        
            // Также логируем в отдельный файл для каждого метода
            var methodLogPath = Path.Combine(Directory.GetCurrentDirectory(), $"collection_passes_{method}.log");
            File.AppendAllText(methodLogPath, logMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error logging collection passes: {ex.Message}");
        }
    }
}