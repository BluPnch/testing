using BenchmarkDotNet.Attributes;

namespace LINQBenchmark.net8.Benchmarks;

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
    public Dictionary<string, int> CountBy_ToLookup()
    {
        return _testData
            .ToLookup(x => x.Category)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    [Benchmark]
    public Dictionary<string, int> CountBy_Manual()
    {
        var dict = new Dictionary<string, int>();
        foreach (var item in _testData)
        {
            dict.TryGetValue(item.Category, out int count);
            dict[item.Category] = count + 1;
        }
        return dict;
    }
}