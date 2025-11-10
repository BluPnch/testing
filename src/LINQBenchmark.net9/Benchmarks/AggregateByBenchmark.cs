using BenchmarkDotNet.Attributes;
using LINQBenchmark.Benchmarks;


namespace LINQBenchmark.net9.Benchmarks;

public class AggregateByBenchmark : BaseBenchmark
{
    [Benchmark(Baseline = true)]
    public Dictionary<string, decimal> AggregateBy_Net8_GroupBy()
    {
        return _testData
            .GroupBy(x => x.Category)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Value));
    }

#if NET9_0
    [Benchmark]
    public Dictionary<string, decimal> AggregateBy_Net9_Optimized()
    {
        return _testData
            .AggregateBy(
                keySelector: x => x.Category,
                seed: 0m,
                (total, item) => total + item.Value)
            .ToDictionary(x => x.Key, x => x.Value);
    }
#endif
}