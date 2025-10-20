using Domain.Models;

namespace Domain.Interfaces.Repositories
{
    public interface IAuthUserRepository
    {
        Task<AuthUser?> GetByUsernameAsync(string username);
        Task<AuthUser?> GetByUserIdAsync(Guid id);
        Task<AuthUser?> GetByIdAsync(Guid id);
        Task<bool> UsernameExistsAsync(string username);
        Task CreateAsync(AuthUser authUser);
        Task UpdateAsync(AuthUser authUser);
        Task<IEnumerable<AuthUser>> GetAllAsync();
    }
} 