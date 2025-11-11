using ContinuousMetricsService.net9.Models;

namespace ContinuousMetricsService.net9.Helpers;

public static class DataGenerator
{
    public static TestData[] GenerateTestData(int size)
    {
        var random = new Random();
        var categories = new[] { "Electronics", "Books", "Clothing", "Food", "Toys" };
        var testData = new TestData[size];

        for (int i = 0; i < size; i++)
        {
            testData[i] = new TestData
            {
                Id = i + 1,
                Category = categories[random.Next(categories.Length)],
                Value = (decimal)(random.NextDouble() * 1000), // Random value between 0 and 1000
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(365))
            };
        }

        return testData; // ✅ Return statement added
    }
}