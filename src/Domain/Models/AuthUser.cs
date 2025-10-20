using Domain.Models.Enums;


namespace Domain.Models
{
    public class AuthUser
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public EnumAuth Role { get; set; }
    }
} 