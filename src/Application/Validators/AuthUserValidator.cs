using Domain.Models;
using FluentValidation;

namespace Application.Validators
{
    public class AuthUserValidator : AbstractValidator<AuthUser>
    {
        public AuthUserValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters")
                .MaximumLength(50).WithMessage("Username must not exceed 50 characters");

            RuleFor(x => x.PasswordHash)
                .NotEmpty().WithMessage("Password is required");

            RuleFor(x => x.Role)
                .IsInEnum().WithMessage("Invalid role");
        }
    }
} 