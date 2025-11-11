namespace ContinuousMetricsService.net9.Models;
public class TestData
{
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public DateTime CreatedAt { get; set; }
}