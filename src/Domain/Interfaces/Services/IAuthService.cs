using Domain.Models;
using Domain.Models.Enums;

namespace Domain.Interfaces
{
    public interface IAuthService
    {
        Task<string> LoginAsync(string username, string password);
        Task<AuthUser> RegisterAsync(string email, string password);
        Task<AuthUser> UpdateUserRoleAsync(Guid userId, EnumAuth newRole, Guid? administratorId = null);
        Task<AuthUser> GetUserByIdAsync(Guid userId);
        Task<AuthUser> GetUserByUsernameAsync(string username);
        Task<IEnumerable<AuthUser>> GetAllAuthUsersAsync();
    }
} 