using Domain.Interfaces.Repositories;
using Domain.Models;
using DataAccess.Context;
using DataAccess.Models.Converters;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class AdministratorRepository : IAdministratorRepository
    {
        private readonly GreenhouseContext _context;

        public AdministratorRepository(GreenhouseContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Administrator>> GetAllAdministratorsAsync()
        {
            var administrators = await _context.Administrators.ToListAsync();
            return administrators.Select(a => a.ToDomain());
        }

        public async Task<Administrator> GetAdministratorByIdAsync(Guid id)
        {
            var administrator = await _context.Administrators.FindAsync(id);
            if (administrator == null)
            {
                throw new AdministratorNotFoundException($"Administrator not found with id = {id}");
            }
            
            return administrator.ToDomain();
        }

        public async Task<Administrator> GetAdministratorByPhoneNumberAsync(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ArgumentException("Phone number cannot be empty", nameof(phoneNumber));
            }

            var administrator = await _context.Administrators
                .FirstOrDefaultAsync(a => a.PhoneNumber == phoneNumber);
                
            if (administrator == null)
            {
                throw new AdministratorNotFoundException($"Administrator with phone number {phoneNumber} not found");
            }
            
            return administrator.ToDomain();
        }

        public async Task<Administrator> GetAdministratorByFullNameAsync(string surname, string name, string patronymic)
        {
            var administrator = await _context.Administrators
                .FirstOrDefaultAsync(a => 
                    a.Surname == surname && 
                    a.Name == name && 
                    a.Patronymic == patronymic);
                    
            if (administrator == null)
            {
                throw new AdministratorNotFoundException($"Administrator with {surname} {name} {patronymic} not found");
            }
            
            return administrator.ToDomain();
        }


        public async Task<Administrator> UpdateAdministratorAsync(Administrator administrator)
        {
            if (administrator == null)
                throw new ArgumentNullException(nameof(administrator));

            var existingAdmin = await _context.Administrators
                .FirstOrDefaultAsync(a => a.Id == administrator.Id);

            if (existingAdmin == null)
                throw new AdministratorNotFoundException($"Administrator with id {administrator.Id} not found");

            existingAdmin.PhoneNumber = administrator.PhoneNumber;
            existingAdmin.Surname = administrator.Surname;
            existingAdmin.Name = administrator.Name;
            existingAdmin.Patronymic = administrator.Patronymic;
            existingAdmin.Username = administrator.Username;

            _context.Administrators.Update(existingAdmin);
            await _context.SaveChangesAsync();

            return existingAdmin.ToDomain();
        }

        // public async Task DeleteAdministratorAsync(Guid id)
        // {
        //     var administrator = await _context.Administrators.FindAsync(id);
        //     if (administrator == null)
        //     {
        //         throw new AdministratorNotFoundException($"Administrator not found with id = {id}");
        //     }
        //
        //     _context.Administrators.Remove(administrator);
        //     await _context.SaveChangesAsync();
        // }
        
        public async Task<Administrator> CreateAdministratorAsync(Guid id, string phoneNumber, string surname, string name, string? patronymic, string username)
        {
            var administrator = new Administrator
            (
                id,
                surname,
                name,
                patronymic,
                phoneNumber,
                username
            );
        
            var administratorDb = administrator.ToDb();
            await _context.Administrators.AddAsync(administratorDb!);
            await _context.SaveChangesAsync();
        
            return administratorDb.ToDomain();
        }
    }
}