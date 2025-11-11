using System.Diagnostics;
using System.Diagnostics.Metrics;
using BenchmarkDotNet.Attributes;


namespace LINQBenchmark.net8.Benchmarks;

public class CountByBenchmark : BaseBenchmark
{
    [Benchmark(Baseline = true)]
    public Dictionary<string, int> CountBy_GroupBy()
    {
        // Считаем обходы коллекции
        CollectionPassesCounter.Add(2); // GroupBy + ToDictionary
        
        var result = _testData
            .GroupBy(x => x.Category)
            .ToDictionary(g => g.Key, g => g.Count());
            
        // Считаем аллокации
        MemoryAllocationsCounter.Add(GC.GetTotalAllocatedBytes(true));
        AllocationsCountCounter.Add(1);
        
        return result;
    }

    [Benchmark]
    public Dictionary<string, int> CountBy_ToLookup()
    {
        CollectionPassesCounter.Add(2); // ToLookup + ToDictionary
        
        var result = _testData
            .ToLookup(x => x.Category)
            .ToDictionary(g => g.Key, g => g.Count());
            
        MemoryAllocationsCounter.Add(GC.GetTotalAllocatedBytes(true));
        AllocationsCountCounter.Add(1);
        
        return result;
    }

    [Benchmark]
    public Dictionary<string, int> CountBy_Manual()
    {
        CollectionPassesCounter.Add(1); // Один обход
        
        var dict = new Dictionary<string, int>();
        foreach (var item in _testData)
        {
            dict.TryGetValue(item.Category, out int count);
            dict[item.Category] = count + 1;
        }
        
        MemoryAllocationsCounter.Add(GC.GetTotalAllocatedBytes(true));
        AllocationsCountCounter.Add(1);
        
        return dict;
    }
}