using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using LINQBenchmark.Helpers;
using LINQBenchmark.Models;

namespace LINQBenchmark.Benchmarks;

[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
[MemoryDiagnoser]
public abstract class BaseBenchmark
{
    protected const int DataSize = 10000;
    protected TestData[] _testData = null!;

    [GlobalSetup]
    public void Setup()
    {
        _testData = DataGenerator.GenerateTestData(DataSize);
    }
}