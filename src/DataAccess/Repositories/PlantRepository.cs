using Domain.Interfaces.Repositories;
using Domain.Models;
using DataAccess.Context;
using DataAccess.Models.Converters;
using Microsoft.EntityFrameworkCore;
using Domain.Exceptions;

namespace DataAccess.Repositories
{
    public class PlantRepository : IPlantRepository
    {
        private readonly GreenhouseContext _context;

        public PlantRepository(GreenhouseContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Plant> CreatePlantAsync(Plant plant, Guid clientId)
        {
            if (plant == null)
                throw new ArgumentNullException(nameof(plant));

            var plantDb = plant.ToDb(clientId);
            _context.Plants.Add(plantDb!);
            await _context.SaveChangesAsync();
            return plantDb!.ToDomain()!;
        }

        public async Task<Plant> GetPlantByIdAsync(Guid id)
        {
            var plant = await _context.Plants
                .AsNoTracking()
                .Include(p => p.JournalRecords)
                .Include(p => p.Seeds)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (plant == null)
                throw new PlantNotFoundException($"Plant with id '{id}' not found");

            return plant.ToDomain()!;
        }

        public async Task<IEnumerable<Plant>> GetAllPlantsAsync()
        {
            var plants = await _context.Plants
                .AsNoTracking()
                .Include(p => p.Client)
                .ToListAsync();

            return plants.ToDomain();
        }

        public async Task<Plant> UpdatePlantAsync(Plant plant)
        {
            if (plant == null)
                throw new ArgumentNullException(nameof(plant));

            var existingPlant = await _context.Plants.FindAsync(plant.Id);
            if (existingPlant == null)
                throw new PlantNotFoundException($"Plant with id '{plant.Id}' not found");

            existingPlant.Specie = plant.Specie;
            existingPlant.Family = plant.Family;
            existingPlant.Flower = plant.Flower;
            existingPlant.Fruit = plant.Fruit;
            existingPlant.Reproduction = plant.Reproduction;

            _context.Plants.Update(existingPlant);
            await _context.SaveChangesAsync();

            return existingPlant.ToDomain()!;
        }

        public async Task DeletePlantAsync(Guid id)
        {
            var plant = await _context.Plants.FindAsync(id);
            if (plant == null)
                throw new PlantNotFoundException($"Plant with id '{id}' not found");

            _context.Plants.Remove(plant);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Plant>> GetPlantsByFamilyAsync(string family)
        {
            var plants = await _context.Plants
                .AsNoTracking()
                .Where(p => p.Family == family)
                .ToListAsync();

            return plants.ToDomain();
        }

        public async Task<IEnumerable<Plant>> GetPlantsBySpeciesAsync(string species)
        {
            var plants = await _context.Plants
                .AsNoTracking()
                .Where(p => p.Specie == species)
                .ToListAsync();

            return plants.ToDomain();
        }

        public async Task<IEnumerable<JournalRecord>> GetJournalRecordsByPlantIdAsync(Guid plantId)
        {
            var records = await _context.JournalRecords
                .AsNoTracking()
                .Include(j => j.GrowthStage)
                .Where(j => j.PlantId == plantId)
                .ToListAsync();

            var sortedRecords = records
                .OrderByDescending(j => j.Date)
                .ToList();

            return sortedRecords.ToDomain();
        }

        public async Task<IEnumerable<Seed>> GetSeedsByPlantIdAsync(Guid plantId)
        {
            var seeds = await _context.Seeds
                .AsNoTracking()
                .Where(s => s.PlantId == plantId)
                .ToListAsync();

            return seeds.ToDomain();
        }

        public async Task<IEnumerable<Plant>> GetPlantsByClientIdAsync(Guid clientId)
        {
            var plants = await _context.Plants
                .AsNoTracking()
                .Where(p => p.ClientId == clientId)
                .ToListAsync();

            return plants.ToDomain();
        }
    }
}