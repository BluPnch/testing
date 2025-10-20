using DataAccess.Context;
using DataAccess.Models;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class AuthUserRepository : IAuthUserRepository
    {
        private readonly GreenhouseContext _context;
        private IAuthUserRepository _authUserRepositoryImplementation;

        public AuthUserRepository(GreenhouseContext context)
        {
            _context = context;
        }

        public async Task<AuthUser?> GetByUsernameAsync(string username)
        {
            var authUserDb = await _context.AuthUsers
                .FirstOrDefaultAsync(u => u.Username == username);

             if (authUserDb == null)
                return null;

            return new AuthUser
            {
                Id = authUserDb.Id,
                Username = authUserDb.Username,
                PasswordHash = authUserDb.PasswordHash,
                Role = authUserDb.Role
            };
        }

        public async Task<AuthUser?> GetByUserIdAsync(Guid id)
        {
            var authUserDb = await _context.AuthUsers
                .FirstOrDefaultAsync(u => u.Id == id);

            if (authUserDb == null)
                return null;

            return new AuthUser
            {
                Id = authUserDb.Id,
                Username = authUserDb.Username,
                PasswordHash = authUserDb.PasswordHash,
                Role = authUserDb.Role
            };
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.AuthUsers
                .AnyAsync(u => u.Username == username);
        }

        public async Task CreateAsync(AuthUser authUser)
        {
            var authUserDb = new AuthUserDb
            {
                Id = authUser.Id,
                Username = authUser.Username,
                PasswordHash = authUser.PasswordHash,
                Role = authUser.Role
            };

            await _context.AuthUsers.AddAsync(authUserDb);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(AuthUser authUser)
        {
            var authUserDb = await _context.AuthUsers.FindAsync(authUser.Id);
            if (authUserDb == null)
            {
                throw new InvalidOperationException($"AuthUser with id {authUser.Id} not found");
            }

            authUserDb.Role = authUser.Role;
            await _context.SaveChangesAsync();
        }

        public async Task<AuthUser?> GetByIdAsync(Guid id)
        {
            var authUserDb = await _context.AuthUsers.FindAsync(id);
            if (authUserDb == null)
                return null;

            return new AuthUser
            {
                Id = authUserDb.Id,
                Username = authUserDb.Username,
                PasswordHash = authUserDb.PasswordHash,
                Role = authUserDb.Role
            };
        }

        public async Task<IEnumerable<AuthUser>> GetAllAsync()
        {
            var authUsersDb = await _context.AuthUsers
                .AsNoTracking()
                .ToListAsync();

            return authUsersDb.Select(u => new AuthUser
            {
                Id = u.Id,
                Username = u.Username,
                PasswordHash = u.PasswordHash,
                Role = u.Role
            });
        }
    }
} 