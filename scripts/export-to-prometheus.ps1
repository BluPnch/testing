param(
    [string]$ResultsPath = "results"
)

# Читаем последний JSON файл с результатами
$latestFile = Get-ChildItem $ResultsPath -Filter "benchmark_*.json" | 
              Sort-Object LastWriteTime -Descending | 
              Select-Object -First 1

if (-not $latestFile) {
    Write-Host "No benchmark results found!" -ForegroundColor Red
    return
}

Write-Host "Processing: $($latestFile.Name)" -ForegroundColor Green

$data = Get-Content $latestFile.FullName | ConvertFrom-Json

# Создаем Prometheus-совместимые метрики
$prometheusMetrics = @()

foreach ($benchmark in $data) {
    $benchmarkName = $benchmark.Benchmark
    $timestamp = [Math]::Round((Get-Date $benchmark.Timestamp -UFormat %s))
    
    foreach ($result in $benchmark.Results) {
        $method = $result.Method
        
        # Метрика памяти
        $prometheusMetrics += "benchmark_memory_bytes{benchmark=`"$benchmarkName`",method=`"$method`"} $($result.Allocated) $timestamp"
        
        # Метрика аллокаций (считаем каждую операцию как 1 аллокацию)
        $prometheusMetrics += "benchmark_allocations_count{benchmark=`"$benchmarkName`",method=`"$method`"} 1 $timestamp"
        
        # Метрика времени выполнения (наносекунды)
        $prometheusMetrics += "benchmark_duration_ns{benchmark=`"$benchmarkName`",method=`"$method`"} $($result.Mean) $timestamp"
        
        # GC collections
        $prometheusMetrics += "benchmark_gc_gen0{benchmark=`"$benchmarkName`",method=`"$method`"} $($result.Gen0Collections) $timestamp"
        $prometheusMetrics += "benchmark_gc_gen1{benchmark=`"$benchmarkName`",method=`"$method`"} $($result.Gen1Collections) $timestamp"
        $prometheusMetrics += "benchmark_gc_gen2{benchmark=`"$benchmarkName`",method=`"$method`"} $($result.Gen2Collections) $timestamp"
    }
}

# Сохраняем в файл для Prometheus
$outputFile = "prometheus_metrics.txt"
$prometheusMetrics | Out-File $outputFile -Encoding UTF8

Write-Host "Metrics exported to: $outputFile" -ForegroundColor Green
Write-Host "Total metrics: $($prometheusMetrics.Count)" -ForegroundColor Cyan