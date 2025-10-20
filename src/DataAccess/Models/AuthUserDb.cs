using Domain.Models.Enums;


namespace DataAccess.Models
{
    public class AuthUserDb
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public EnumAuth Role { get; set; }
    }
} 