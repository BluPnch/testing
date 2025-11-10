using System.Text.Json;
using BenchmarkDotNet.Running;
using LINQBenchmark.net9.Benchmarks;


namespace LINQBenchmark.Runner.net9;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("=== LINQ Performance Benchmark (.NET 9) ===");
            Console.WriteLine($"Running on .NET {Environment.Version}");
            Console.WriteLine($"OS: {Environment.OSVersion}");
            Console.WriteLine($"Processor Count: {Environment.ProcessorCount}");
            Console.WriteLine();

            var results = new List<object>();
            
            // Запускаем CountBy benchmark
            Console.WriteLine("Running CountBy benchmarks...");
            try
            {
                var countBySummary = BenchmarkRunner.Run<CountByBenchmark>();
                results.Add(new {
                    Benchmark = "CountBy",
                    Results = ProcessSummary(countBySummary)
                });
                Console.WriteLine("✓ CountBy completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ CountBy failed: {ex.Message}");
            }

            // Запускаем AggregateBy benchmark
            Console.WriteLine("Running AggregateBy benchmarks...");
            try
            {
                var aggregateBySummary = BenchmarkRunner.Run<AggregateByBenchmark>();
                results.Add(new {
                    Benchmark = "AggregateBy", 
                    Results = ProcessSummary(aggregateBySummary)
                });
                Console.WriteLine("✓ AggregateBy completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ AggregateBy failed: {ex.Message}");
            }

            // Сохраняем результаты
            SaveResults(results);
            
            Console.WriteLine("All benchmarks completed!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex}");
            Environment.Exit(1);
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