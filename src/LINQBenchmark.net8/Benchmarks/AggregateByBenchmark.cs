using BenchmarkDotNet.Attributes;

namespace LINQBenchmark.net8.Benchmarks;

public class AggregateByBenchmark : BaseBenchmark
{
    [Benchmark(Baseline = true)]
    public Dictionary<string, decimal> AggregateBy_GroupBy()
    {
        return _testData
            .GroupBy(x => x.Category)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Value));
    }

    [Benchmark]
    public Dictionary<string, decimal> AggregateBy_Aggregate()
    {
        return _testData
            .GroupBy(x => x.Category)
            .ToDictionary(
                g => g.Key, 
                g => g.Aggregate(0m, (total, item) => total + item.Value)
            );
    }

    [Benchmark]
    public Dictionary<string, decimal> AggregateBy_Select()
    {
        return _testData
            .GroupBy(x => x.Category)
            .Select(g => new { Key = g.Key, Sum = g.Sum(x => x.Value) })
            .ToDictionary(x => x.Key, x => x.Sum);
    }
}