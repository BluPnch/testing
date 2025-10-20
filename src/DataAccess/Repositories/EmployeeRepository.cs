using Domain.Interfaces.Repositories;
using Domain.Models;
using DataAccess.Context;
using DataAccess.Models;
using DataAccess.Models.Converters;
using Microsoft.EntityFrameworkCore;
using Domain.Exceptions;

namespace DataAccess.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly GreenhouseContext _context;

        public EmployeeRepository(GreenhouseContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Employee> CreateEmployeeAsync(Employee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            var employeeDb = employee.ToDb();
            await _context.Employees.AddAsync(employeeDb!);
            await _context.SaveChangesAsync();

            return employeeDb.ToDomain()!;
        }

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {
            var employees = await _context.Employees
                .AsNoTracking()
                .ToListAsync();

            return employees.ToDomain();
        }

        public async Task<Employee> GetEmployeeByIdAsync(Guid id)
        {
            var employee = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
                throw new EmployeeNotFoundException($"Employee with id '{id}' not found");

            return employee.ToDomain()!;
        }

        public async Task DeleteEmployeeAsync(Guid id)
        {
            var employee = await _context.Employees.FindAsync(id);
            
            if (employee == null)
                throw new EmployeeNotFoundException($"Employee with id '{id}' not found");

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Employee>> GetEmployeesByTaskAsync(string task)
        {
            var employees = await _context.Employees
                .AsNoTracking()
                .Where(e => e.Task == task)
                .ToListAsync();

            return employees.ToDomain();
        }

        public async Task<IEnumerable<Employee>> GetEmployeesByPlantDomainAsync(string plantDomain)
        {
            var employees = await _context.Employees
                .AsNoTracking()
                .Where(e => e.PlantDomain == plantDomain)
                .ToListAsync();

            return employees.ToDomain();
        }

        public async Task<Employee> GetEmployeeByPhoneNumberAsync(string phoneNumber)
        {
            var employee = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.PhoneNumber == phoneNumber);

            if (employee == null)
                throw new EmployeeNotFoundException($"Employee with phone number {phoneNumber} not found");

            return employee.ToDomain()!;
        }

        public async Task<IEnumerable<Plant>> GetPlantsByEmployeeIdAsync(Guid employeeId)
        {
            var plants = await _context.EmployeePlants
                .Where(ep => ep.EmployeeId == employeeId)
                .Select(ep => ep.Plant)
                .AsNoTracking()
                .ToListAsync();

            return plants.ToDomain();
        }

        public async Task AssignEmployeeToPlantAsync(Guid employeeId, Guid plantId)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
                throw new EmployeeNotFoundException($"Employee with id '{employeeId}' not found");

            var plant = await _context.Plants.FindAsync(plantId);
            if (plant == null)
                throw new PlantNotFoundException($"Plant with id '{plantId}' not found");
            
            var existingAssignment = await _context.EmployeePlants
                .FirstOrDefaultAsync(ep => ep.EmployeeId == employeeId && ep.PlantId == plantId);

            if (existingAssignment != null)
                return;

            var employeePlant = new EmployeePlantDb
            {
                EmployeeId = employeeId,
                PlantId = plantId
            };

            await _context.EmployeePlants.AddAsync(employeePlant);
            await _context.SaveChangesAsync();
        }
    }
}