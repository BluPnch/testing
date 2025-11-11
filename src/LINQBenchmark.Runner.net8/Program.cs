using BenchmarkDotNet.Running;
using LINQBenchmark.net8.Benchmarks;
using System.Text.Json;
using BenchmarkDotNet.Configs;


namespace LINQBenchmark.Runner.net8;

class Program
{
    static void Main(string[] args)
    {
        bool quickMode = args.Contains("--quick") || args.Contains("-q");
    
        if (quickMode)
        {
            Console.WriteLine("=== QUICK MODE - Reduced iterations ===");
            // Можно передать параметры в BenchmarkDotNet
            var config = DefaultConfig.Instance.WithOption(ConfigOptions.DisableOptimizationsValidator, true);
        }
        
        try
        {
            Console.WriteLine("=== LINQ Performance Benchmark .NET 8 ===");
            Console.WriteLine($"Running on .NET {Environment.Version}");
            
            var results = new List<object>();

            // CountBy benchmarks
            Console.WriteLine("Running CountBy benchmarks...");
            var countBySummary = BenchmarkRunner.Run<CountByBenchmark>();
            results.Add(new {
                Benchmark = "CountBy",
                Results = ProcessSummary(countBySummary),
                Timestamp = DateTime.UtcNow
            });

            // AggregateBy benchmarks  
            Console.WriteLine("Running AggregateBy benchmarks...");
            var aggregateBySummary = BenchmarkRunner.Run<AggregateByBenchmark>();
            results.Add(new {
                Benchmark = "AggregateBy", 
                Results = ProcessSummary(aggregateBySummary),
                Timestamp = DateTime.UtcNow
            });

            SaveResults(results);
            Console.WriteLine("All benchmarks completed!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex}");
        }
    }

    static object ProcessSummary(BenchmarkDotNet.Reports.Summary summary)
    {
        if (summary?.Reports == null)
            return new { Error = "No reports generated" };

        return summary.Reports.Select(r => new
        {
            Method = r.BenchmarkCase.Descriptor.WorkloadMethod.Name,
            Mean = r.ResultStatistics?.Mean,
            Median = r.ResultStatistics?.Median,
            StdDev = r.ResultStatistics?.StandardDeviation,
            Min = r.ResultStatistics?.Min,
            Max = r.ResultStatistics?.Max,
            Allocated = r.GcStats.GetTotalAllocatedBytes(true),
            Gen0Collections = r.GcStats.Gen0Collections,
            Gen1Collections = r.GcStats.Gen1Collections,
            Gen2Collections = r.GcStats.Gen2Collections
        }).ToList();
    }

    static void SaveResults(List<object> results)
    {
        try
        {
            var resultsDir = Path.Combine(Directory.GetCurrentDirectory(), "results");
            Directory.CreateDirectory(resultsDir);
            
            var fileName = $"benchmark_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            var filePath = Path.Combine(resultsDir, fileName);
            
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(filePath, JsonSerializer.Serialize(results, options));
            
            Console.WriteLine($"Results saved to: {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving results: {ex.Message}");
        }
    }
}