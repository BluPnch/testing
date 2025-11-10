using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using LINQBenchmark.Helpers;
using LINQBenchmark.Models;

namespace LINQBenchmark.net8.Benchmarks;

[SimpleJob(RuntimeMoniker.Net80, baseline: true)]
[MemoryDiagnoser]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
public abstract class BaseBenchmark
{
    protected const int DataSize = 10000;
    protected TestData[] _testData = null!;

    [GlobalSetup]
    public void Setup()
    {
        _testData = DataGenerator.GenerateTestData(DataSize);
        Console.WriteLine($"Generated {_testData.Length} test items");
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _testData = null;
        GC.Collect();
    }
}