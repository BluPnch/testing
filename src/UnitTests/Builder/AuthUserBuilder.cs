using Domain.Models;
using Domain.Models.Enums;

namespace UnitTests.Builders
{
    public class AuthUserBuilder
    {
        private Guid _id = Guid.NewGuid();
        private string _username = "user";
        private string _passwordHash = "hashed_password";
        private EnumAuth _role = EnumAuth.Administrator;

        public AuthUserBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public AuthUserBuilder WithUsername(string username)
        {
            _username = username;
            return this;
        }

        public AuthUserBuilder WithPasswordHash(string passwordHash)
        {
            _passwordHash = passwordHash;
            return this;
        }

        public AuthUserBuilder WithRole(EnumAuth role)
        {
            _role = role;
            return this;
        }

        public AuthUser Build()
        {
            return new AuthUser
            {
                Id = _id,
                Username = _username,
                PasswordHash = _passwordHash,
                Role = _role
            };
        }

        public static implicit operator AuthUser(AuthUserBuilder builder)
        {
            return builder.Build();
        }
    }
}