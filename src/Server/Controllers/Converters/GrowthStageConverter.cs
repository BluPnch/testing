using Domain.Models;
using Server.Controllers.Models;

namespace Server.Controllers.Converters;

public static class GrowthStageConverter
{
    public static GrowthStageDTO ToDTO(GrowthStage growthStage)
    {
        if (growthStage == null) return null;
        
        return new GrowthStageDTO
        {
            Id = growthStage.Id,
            Name = growthStage.Name,
            Description = growthStage.Description
        };
    }

    public static GrowthStage ToDomain(GrowthStageDTO dto)
    {
        if (dto == null) return null;
        
        return new GrowthStage(dto.Id, dto.Name, dto.Description);
    }

    public static IEnumerable<GrowthStageDTO> ToDTO(IEnumerable<GrowthStage> growthStages)
    {
        return growthStages?.Select(ToDTO) ?? Enumerable.Empty<GrowthStageDTO>();
    }
}

