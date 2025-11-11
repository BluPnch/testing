using BenchmarkDotNet.Attributes;

using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace LINQBenchmark.net9.Benchmarks;

public class CountByBenchmark : BaseBenchmark
{
    [Benchmark(Baseline = true)]
    public Dictionary<string, int> CountBy_GroupBy()
    {
        CollectionPassesCounter.Add(2); // GroupBy + ToDictionary
        
        var result = _testData
            .GroupBy(x => x.Category)
            .ToDictionary(g => g.Key, g => g.Count());
            
        MemoryAllocationsCounter.Add(GC.GetTotalAllocatedBytes(true));
        AllocationsCountCounter.Add(1);
        
        return result;
    }

    [Benchmark]
    public Dictionary<string, int> CountBy_Net9()
    {
        CollectionPassesCounter.Add(1); // CountBy один обход
        
        var result = _testData
            .CountBy(x => x.Category)
            .ToDictionary(x => x.Key, x => x.Value);
            
        MemoryAllocationsCounter.Add(GC.GetTotalAllocatedBytes(true));
        AllocationsCountCounter.Add(1);
        
        return result;
    }
}