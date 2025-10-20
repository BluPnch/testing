using Domain.Models;
using FluentValidation;


namespace Application.Validators
{
    public class ClientValidator : AbstractValidator<Client>
    {
        public ClientValidator()
        {
            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("Название компании не может быть пустым")
                .Length(1, 50).WithMessage("Название компании должно быть от 1 до 50 символов")
                .Matches(@"^[а-яА-ЯёЁa-zA-Z\-]+$").WithMessage("Название компании может содержать только буквы и дефис");
            
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Номер телефона не может быть пустой")
                .Matches(@"^\+?[0-9\s\-\(\)]{7,20}$")
                .WithMessage("Номер телефона должен быть в международном или местном формате");
        }
    }
}