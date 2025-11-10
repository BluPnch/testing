using System;
using LINQBenchmark.Models;


namespace LINQBenchmark.Helpers;

public static class DataGenerator
{
    private static readonly string[] Categories = { "A", "B", "C", "D", "E", "F", "G" };
    private static readonly Random Random = new Random(42); // Фиксированный seed для воспроизводимости

    public static TestData[] GenerateTestData(int size)
    {
        var data = new TestData[size];
        
        for (int i = 0; i < size; i++)
        {
            data[i] = new TestData
            {
                Id = i,
                Category = Categories[Random.Next(Categories.Length)],
                Value = (decimal)(Random.NextDouble() * 1000),
                CreatedAt = DateTime.Now.AddDays(-Random.Next(365))
            };
        }
        
        return data;
    }
}