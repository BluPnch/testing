using Domain.Models;


namespace DataAccess.Models.Converters;


public static class GrowthStageConverter
{
    public static GrowthStage? ToDomain(this GrowthStageDb? stage)
    {
        if (stage is null) return null;

        return new GrowthStage(
            id: stage.Id,
            name: stage.Name,
            description: stage.Description
        );
    }

    public static GrowthStageDb? ToDb(this GrowthStage? stage)
    {
        if (stage is null) return null;

        return new GrowthStageDb(
            id: stage.Id,
            name: stage.Name,
            description: stage.Description
        );
    }

    public static IEnumerable<GrowthStage> ToDomain(this IEnumerable<GrowthStageDb> stages)
        => stages.Select(s => s.ToDomain())!;
}