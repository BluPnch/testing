using System.Diagnostics.Metrics;
using ContinuousMetricsService.net9.Helpers;
using ContinuousMetricsService.net9.Models;


namespace ContinuousMetricsService.net9.Services;

public class ContinuousMetricsService : BackgroundService
{
    private readonly TestData[] _testData;
    private readonly ILogger<ContinuousMetricsService> _logger;
    private readonly string _runtimeVersion;

    // Метрики для Prometheus
    private static readonly Meter Meter = new("LINQBenchmark");
    private static readonly Counter<int> EnumeratorCreationsCounter = Meter.CreateCounter<int>("linq_enumerator_creations");
    private static readonly Counter<int> MoveNextOperationsCounter = Meter.CreateCounter<int>("linq_move_next_operations");
    private static readonly Counter<long> MemoryAllocatedBytesCounter = Meter.CreateCounter<long>("linq_memory_allocated_bytes");
    private static readonly Counter<int> MethodInvocationsCounter = Meter.CreateCounter<int>("linq_method_invocations");
    private static readonly Histogram<double> ExecutionDurationHistogram = Meter.CreateHistogram<double>("linq_execution_duration_ms");

    public ContinuousMetricsService(ILogger<ContinuousMetricsService> logger)
    {
        _logger = logger;
        _testData = DataGenerator.GenerateTestData(1000);
        _runtimeVersion = "net9";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting continuous metrics collection for {Runtime}", _runtimeVersion);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunAllBenchmarks();
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during benchmark execution");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }

    private async Task RunAllBenchmarks()
    {
        await RunCountByBenchmarks();
        await RunAggregateByBenchmarks();
    }

    private async Task RunCountByBenchmarks()
    {
        await Task.Run(() =>
        {
            RunAndRecordMetrics("CountBy_GroupBy", () => CountBy_GroupBy(_testData));
            RunAndRecordMetrics("CountBy_ToLookup", () => CountBy_ToLookup(_testData));
            RunAndRecordMetrics("CountBy_Net9", () => CountBy_Net9(_testData));
        });
    }

    private async Task RunAggregateByBenchmarks()
    {
        await Task.Run(() =>
        {
            RunAndRecordMetrics("AggregateBy_GroupBy", () => AggregateBy_GroupBy(_testData));
            RunAndRecordMetrics("AggregateBy_Aggregate", () => AggregateBy_Aggregate(_testData));
            RunAndRecordMetrics("AggregateBy_Net9", () => AggregateBy_Net9(_testData));
        });
    }

    private void RunAndRecordMetrics(string methodName, Func<object> benchmarkAction)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            CountingEnumerable<TestData>.ResetCounters();
            var initialMemory = GC.GetAllocatedBytesForCurrentThread();

            var result = benchmarkAction();

            var allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - initialMemory;
            var durationMs = stopwatch.Elapsed.TotalMilliseconds;

            var tags = new KeyValuePair<string, object?>[] 
            {
                new("method", methodName),
                new("runtime", _runtimeVersion)
            };

            EnumeratorCreationsCounter.Add(CountingEnumerable<TestData>.EnumeratorCount, tags);
            MoveNextOperationsCounter.Add(CountingEnumerable<TestData>.MoveNextCount, tags);
            MemoryAllocatedBytesCounter.Add(allocatedBytes, tags);
            MethodInvocationsCounter.Add(1, tags);
            ExecutionDurationHistogram.Record(durationMs, tags);

            _logger.LogDebug(
                "Method {Method} completed: {Enumerators} enumerators, {MoveNext} moves, {Memory} bytes, {Duration}ms",
                methodName, CountingEnumerable<TestData>.EnumeratorCount, CountingEnumerable<TestData>.MoveNextCount,
                allocatedBytes, durationMs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing method {Method}", methodName);
        }
    }

    // CountBy методы
    private Dictionary<string, int> CountBy_GroupBy(TestData[] data)
    {
        var countingData = new CountingEnumerable<TestData>(data);
        return countingData
            .GroupBy(x => x.Category)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    private Dictionary<string, int> CountBy_ToLookup(TestData[] data)
    {
        var countingData = new CountingEnumerable<TestData>(data);
        return countingData
            .ToLookup(x => x.Category)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    private Dictionary<string, int> CountBy_Net9(TestData[] data)
    {
        var countingData = new CountingEnumerable<TestData>(data);
        return countingData
            .CountBy(x => x.Category)
            .ToDictionary(x => x.Key, x => x.Value);
    }

    // AggregateBy методы
    private Dictionary<string, decimal> AggregateBy_GroupBy(TestData[] data)
    {
        var countingData = new CountingEnumerable<TestData>(data);
        return countingData
            .GroupBy(x => x.Category)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Value));
    }

    private Dictionary<string, decimal> AggregateBy_Aggregate(TestData[] data)
    {
        var countingData = new CountingEnumerable<TestData>(data);
        return countingData
            .GroupBy(x => x.Category)
            .ToDictionary(g => g.Key, g => g.Aggregate(0m, (total, item) => total + item.Value));
    }

    private Dictionary<string, decimal> AggregateBy_Net9(TestData[] data)
    {
        var countingData = new CountingEnumerable<TestData>(data);
        return countingData
            .AggregateBy(keySelector: x => x.Category, seed: 0m, (total, item) => total + item.Value)
            .ToDictionary(x => x.Key, x => x.Value);
    }
}