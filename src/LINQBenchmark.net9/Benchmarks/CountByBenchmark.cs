using BenchmarkDotNet.Attributes;
using LINQBenchmark.Helpers;
using LINQBenchmark.Models;


namespace LINQBenchmark.net9.Benchmarks;

public class CountByBenchmark : BaseBenchmark
{
    [Benchmark(Baseline = true)]
    public Dictionary<string, int> CountBy_GroupBy()
    {
        CountingEnumerable<TestData>.ResetCounters();
        var countingData = new CountingEnumerable<TestData>(_testData);
        
        var result = countingData
            .GroupBy(x => x.Category)
            .ToDictionary(g => g.Key, g => g.Count());
            
        LogCollectionPasses("CountBy_GroupBy", CountingEnumerable<TestData>.EnumeratorCount);
        return result;
    }

    [Benchmark]
    public Dictionary<string, int> CountBy_Net9()
    {
        CountingEnumerable<TestData>.ResetCounters();
        var countingData = new CountingEnumerable<TestData>(_testData);
        
        var result = countingData
            .CountBy(x => x.Category)
            .ToDictionary(x => x.Key, x => x.Value);
            
        LogCollectionPasses("CountBy_Net9", CountingEnumerable<TestData>.EnumeratorCount);
        return result;
    }
}