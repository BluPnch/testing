using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

// Добавляем сервис метрик
builder.Services.AddHostedService<ContinuousMetricsService.net9.Services.ContinuousMetricsService>();

// Настраиваем OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("LINQBenchmark-net9"))
            .AddMeter("LINQBenchmark")
            .AddAspNetCoreInstrumentation();
    });

// Логирование
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Information);
});

var app = builder.Build();

// Создаем свой endpoint для метрик
app.MapGet("/metrics", async (HttpContext context) =>
{
    var meterFactory = app.Services.GetService<MeterProvider>();
    if (meterFactory != null)
    {
        context.Response.ContentType = "text/plain; version=0.0.4; charset=utf-8";
        // Здесь можно добавить логику для экспорта метрик в формате Prometheus
        await context.Response.WriteAsync("# LINQ Benchmark Metrics - Custom Export\n");
        await context.Response.WriteAsync($"# Runtime: NET9\n");
        await context.Response.WriteAsync($"linq_benchmark_running{{runtime=\"net9\"}} 1\n");
    }
});

app.MapGet("/", () => $"LINQ Benchmark Metrics Service - NET9 - {Environment.Version}");
app.MapGet("/health", () => "Healthy");

app.Run();