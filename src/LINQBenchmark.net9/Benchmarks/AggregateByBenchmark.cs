using BenchmarkDotNet.Attributes;
using LINQBenchmark.Helpers;
using LINQBenchmark.Models;

namespace LINQBenchmark.net9.Benchmarks;

public class AggregateByBenchmark : BaseBenchmark
{
    [Benchmark(Baseline = true)]
    public Dictionary<string, decimal> AggregateBy_GroupBy()
    {
        CountingEnumerable<TestData>.ResetCounters();
        var countingData = new CountingEnumerable<TestData>(_testData);
        
        var result = countingData
            .GroupBy(x => x.Category)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Value));
            
        LogCollectionPasses("AggregateBy_GroupBy", CountingEnumerable<TestData>.EnumeratorCount, "net9");
        return result;
    }

    [Benchmark]
    public Dictionary<string, decimal> AggregateBy_Aggregate()
    {
        CountingEnumerable<TestData>.ResetCounters();
        var countingData = new CountingEnumerable<TestData>(_testData);
        
        var result = countingData
            .GroupBy(x => x.Category)
            .ToDictionary(
                g => g.Key, 
                g => g.Aggregate(0m, (total, item) => total + item.Value)
            );
            
        LogCollectionPasses("AggregateBy_Aggregate", CountingEnumerable<TestData>.EnumeratorCount, "net9");
        return result;
    }

    [Benchmark]
    public Dictionary<string, decimal> AggregateBy_Net9()
    {
        CountingEnumerable<TestData>.ResetCounters();
        var countingData = new CountingEnumerable<TestData>(_testData);
        
        var result = countingData
            .AggregateBy(
                keySelector: x => x.Category,
                seed: 0m,
                (total, item) => total + item.Value)
            .ToDictionary(x => x.Key, x => x.Value);
            
        LogCollectionPasses("AggregateBy_Net9", CountingEnumerable<TestData>.EnumeratorCount, "net9");
        return result;
    }
}