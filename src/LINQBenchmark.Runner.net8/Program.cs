using System.Text.Json;
using BenchmarkDotNet.Running;
using LINQBenchmark.net8.Benchmarks;


namespace LINQBenchmark.Runner.net8;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== LINQ Performance Benchmark in Docker ===");
        Console.WriteLine($"Running on .NET {Environment.Version}");
        Console.WriteLine($"OS: {Environment.OSVersion}");
        Console.WriteLine($"Processor Count: {Environment.ProcessorCount}");
        Console.WriteLine();

        var results = new List<object>();
        
        // Запускаем CountBy benchmark
        Console.WriteLine("Running CountBy benchmarks...");
        var countBySummary = BenchmarkRunner.Run<CountByBenchmark>();
        results.Add(new {
            Benchmark = "CountBy",
            Results = ProcessSummary(countBySummary)
        });

        // Запускаем AggregateBy benchmark
        Console.WriteLine("Running AggregateBy benchmarks...");
        var aggregateBySummary = BenchmarkRunner.Run<AggregateByBenchmark>();
        results.Add(new {
            Benchmark = "AggregateBy", 
            Results = ProcessSummary(aggregateBySummary)
        });

        // Сохраняем результаты
        SaveResults(results);
        
        Console.WriteLine("All benchmarks completed!");
    }

    static object ProcessSummary(BenchmarkDotNet.Reports.Summary summary)
    {
        return summary.Reports.Select(r => new
        {
            Method = r.BenchmarkCase.Descriptor.WorkloadMethod.Name,
            Mean = r.ResultStatistics?.Mean,
            Median = r.ResultStatistics?.Median,
            StdDev = r.ResultStatistics?.StandardDeviation,
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