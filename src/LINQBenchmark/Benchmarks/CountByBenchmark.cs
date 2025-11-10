using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace LINQBenchmark.Benchmarks;

public class CountByBenchmark : BaseBenchmark
{
    [Benchmark(Baseline = true)]
    public Dictionary<string, int> CountBy_Net8_GroupBy()
    {
        return _testData
            .GroupBy(x => x.Category)
            .ToDictionary(g => g.Key, g => g.Count());
    }

#if NET9_0
    [Benchmark]
    public Dictionary<string, int> CountBy_Net9_Optimized()
    {
        return _testData
            .CountBy(x => x.Category)
            .ToDictionary(x => x.Key, x => x.Value);
    }
#endif
}