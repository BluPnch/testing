using BenchmarkDotNet.Attributes;

namespace LINQBenchmark.net9.Benchmarks;

public class CountByBenchmark : BaseBenchmark
{
    [Benchmark(Baseline = true)]
    public Dictionary<string, int> CountBy_GroupBy()
    {
        return _testData
            .GroupBy(x => x.Category)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    [Benchmark]
    public Dictionary<string, int> CountBy_Net9()
    {
        return _testData
            .CountBy(x => x.Category)
            .ToDictionary(x => x.Key, x => x.Value);
    }

    [Benchmark]
    public Dictionary<string, int> CountBy_ToLookup()
    {
        return _testData
            .ToLookup(x => x.Category)
            .ToDictionary(g => g.Key, g => g.Count());
    }
}