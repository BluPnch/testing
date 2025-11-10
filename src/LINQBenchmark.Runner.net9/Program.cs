using BenchmarkDotNet.Running;
using LINQBenchmark.net9.Benchmarks;

namespace LINQBenchmark.Runner.net9;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== LINQ Performance Benchmark in Docker ===");
        Console.WriteLine($"Running on .NET {Environment.Version}");
        Console.WriteLine($"OS: {Environment.OSVersion}");
        Console.WriteLine($"Processor Count: {Environment.ProcessorCount}");
        Console.WriteLine();

        // Run CountBy benchmarks
        Console.WriteLine("Running CountBy benchmarks...");
        var countBySummary = BenchmarkRunner.Run<CountByBenchmark>();
        
        // Run AggregateBy benchmarks
        Console.WriteLine("Running AggregateBy benchmarks...");
        var aggregateBySummary = BenchmarkRunner.Run<AggregateByBenchmark>();

        Console.WriteLine("All benchmarks completed!");
    }
}