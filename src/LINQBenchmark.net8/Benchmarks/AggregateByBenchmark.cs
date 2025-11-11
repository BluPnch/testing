using BenchmarkDotNet.Attributes;
using LINQBenchmark.Helpers;
using LINQBenchmark.Models;


namespace LINQBenchmark.net8.Benchmarks;

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
            
        LogCollectionPasses("AggregateBy_GroupBy", CountingEnumerable<TestData>.EnumeratorCount);
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
            
        LogCollectionPasses("AggregateBy_Aggregate", CountingEnumerable<TestData>.EnumeratorCount);
        return result;
    }

    [Benchmark]
    public Dictionary<string, decimal> AggregateBy_Select()
    {
        CountingEnumerable<TestData>.ResetCounters();
        var countingData = new CountingEnumerable<TestData>(_testData);
        
        var result = countingData
            .GroupBy(x => x.Category)
            .Select(g => new { Key = g.Key, Sum = g.Sum(x => x.Value) })
            .ToDictionary(x => x.Key, x => x.Sum);
            
        LogCollectionPasses("AggregateBy_Select", CountingEnumerable<TestData>.EnumeratorCount);
        return result;
    }
}