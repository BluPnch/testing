using DataAccess.Context;
using DataAccess.Models.Converters;
using Domain.Exceptions;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Microsoft.EntityFrameworkCore;


namespace DataAccess.Repositories;

public class GrowthStageRepository : IGrowthStageRepository
{
    private readonly GreenhouseContext _context;

    public GrowthStageRepository(GreenhouseContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    
    public async Task<GrowthStage> CreateGrowthStageAsync(GrowthStage growthStage)
    {
        if (growthStage == null)
            throw new ArgumentNullException(nameof(growthStage));

        var growthStageDb = growthStage.ToDb();
        
        await _context.GrowthStages.AddAsync(growthStageDb!);
        await _context.SaveChangesAsync();

        return growthStageDb.ToDomain()!;
    }
    
    public async Task<IEnumerable<GrowthStage>> GetAllGrowthStagesAsync()
    {
        var growthStages = await _context.GrowthStages
            .AsNoTracking()
            .ToListAsync();

        return growthStages.ToDomain();
    }

    public async Task<GrowthStage> GetGrowthStageByIdAsync(Guid id)
    {
        var growthStage = await _context.GrowthStages
            .AsNoTracking()
            .FirstOrDefaultAsync(gs => gs.Id == id);

        if (growthStage == null)
            throw new GrowthStageNotFoundException($"Growth stage with id '{id}' not found");

        return growthStage.ToDomain()!;
    }
    
    public async Task<GrowthStage> GetGrowthStageByNameAsync(string name)
    {
        var growthStage = await _context.GrowthStages
            .AsNoTracking()
            .FirstOrDefaultAsync(gs => gs.Name == name);

        if (growthStage == null)
            throw new KeyNotFoundException($"Growth stage with name '{name}' not found");

        return growthStage.ToDomain()!;
    }

    public async Task UpdateGrowthStageAsync(GrowthStage growthStage)
    {
        if (growthStage == null)
            throw new ArgumentNullException(nameof(growthStage));

        var existingStage = await _context.GrowthStages
            .FirstOrDefaultAsync(gs => gs.Id == growthStage.Id);

        if (existingStage == null)
            throw new GrowthStageNotFoundException($"Growth stage with id '{growthStage.Id}' not found");

        existingStage.Name = growthStage.Name;
        existingStage.Description = growthStage.Description;

        _context.GrowthStages.Update(existingStage);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteGrowthStageAsync(Guid id)
    {
        var growthStage = await _context.GrowthStages.FindAsync(id);
        
        if (growthStage == null)
            throw new GrowthStageNotFoundException($"Growth stage with id '{id}' not found");

        _context.GrowthStages.Remove(growthStage);
        await _context.SaveChangesAsync();
    }
}