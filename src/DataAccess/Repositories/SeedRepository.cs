using Domain.Interfaces.Repositories;
using Domain.Models;
using DataAccess.Context;
using DataAccess.Models.Converters;
using Microsoft.EntityFrameworkCore;
using Domain.Exceptions;

namespace DataAccess.Repositories
{
    public class SeedRepository : ISeedRepository
    {
        private readonly GreenhouseContext _context;

        public SeedRepository(GreenhouseContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Seed> CreateSeedAsync(Seed seed)
        {
            if (seed == null)
                throw new ArgumentNullException(nameof(seed));

            var seedDb = seed.ToDb();
            await _context.Seeds.AddAsync(seedDb!);
            await _context.SaveChangesAsync();

            return seedDb.ToDomain()!;
        }

        public async Task<Seed> GetSeedByIdAsync(Guid id)
        {
            var seed = await _context.Seeds
                .AsNoTracking()
                .Include(s => s.Plant)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (seed == null)
                throw new SeedNotFoundException($"Seed with id '{id}' not found");

            return seed.ToDomain()!;
        }

        public async Task<IEnumerable<Seed>> GetAllSeedsAsync()
        {
            var seeds = await _context.Seeds
                .AsNoTracking()
                .Include(s => s.Plant)
                .ToListAsync();

            return seeds.ToDomain();
        }

        public async Task DeleteSeedAsync(Guid id)
        {
            var seed = await _context.Seeds.FindAsync(id);
            if (seed == null)
                throw new SeedNotFoundException($"Seed with id '{id}' not found");

            _context.Seeds.Remove(seed);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Seed>> GetSeedsByMaturityAsync(string maturity)
        {
            if (string.IsNullOrWhiteSpace(maturity))
                throw new ArgumentException("Maturity cannot be empty", nameof(maturity));

            var seeds = await _context.Seeds
                .AsNoTracking()
                .Include(s => s.Plant)
                .Where(s => s.Maturity == maturity)
                .ToListAsync();

            return seeds.ToDomain();
        }

        public async Task<IEnumerable<Seed>> GetSeedsByViabilityAsync(string viability)
        {
            if (!Enum.TryParse<EnumViability>(viability, true, out var viabilityEnum))
                throw new ArgumentException("Invalid viability value", nameof(viability));

            var seeds = await _context.Seeds
                .AsNoTracking()
                .Include(s => s.Plant)
                .Where(s => s.Viability == viabilityEnum)
                .ToListAsync();

            return seeds.ToDomain();
        }

        public async Task<Plant> GetPlantBySeedIdAsync(Guid seedId)
        {
            var seed = await _context.Seeds
                .AsNoTracking()
                .Include(s => s.Plant)
                .FirstOrDefaultAsync(s => s.Id == seedId);

            if (seed == null)
                throw new SeedNotFoundException($"Seed with id '{seedId}' not found");

            if (seed.Plant == null)
                throw new PlantNotFoundException($"Plant not found for seed with id {seedId}");

            return seed.Plant.ToDomain()!;
        }

        public async Task<Seed> UpdateSeedAsync(Seed seed)
        {
            if (seed == null)
                throw new ArgumentNullException(nameof(seed));

            var seedDb = seed.ToDb();
            _context.Seeds.Update(seedDb!);
            await _context.SaveChangesAsync();

            return seedDb.ToDomain()!;
        }
    }
}