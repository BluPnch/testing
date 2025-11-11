using BenchmarkDotNet.Attributes;
using LINQBenchmark.Helpers;
using LINQBenchmark.Models;


namespace LINQBenchmark.net8.Benchmarks;

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
    public Dictionary<string, int> CountBy_ToLookup()
    {
        CountingEnumerable<TestData>.ResetCounters();
        var countingData = new CountingEnumerable<TestData>(_testData);
        
        var result = countingData
            .ToLookup(x => x.Category)
            .ToDictionary(g => g.Key, g => g.Count());
            
        LogCollectionPasses("CountBy_ToLookup", CountingEnumerable<TestData>.EnumeratorCount);
        return result;
    }

    [Benchmark]
    public Dictionary<string, int> CountBy_Manual()
    {
        CountingEnumerable<TestData>.ResetCounters();
        var countingData = new CountingEnumerable<TestData>(_testData);
        
        var dict = new Dictionary<string, int>();
        foreach (var item in countingData)
        {
            dict.TryGetValue(item.Category, out int count);
            dict[item.Category] = count + 1;
        }
            
        LogCollectionPasses("CountBy_Manual", CountingEnumerable<TestData>.EnumeratorCount);
        return dict;
    }
}