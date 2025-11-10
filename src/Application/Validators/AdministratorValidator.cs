using Domain.Models;
using FluentValidation;

namespace Application.Validators
{
    public class AdministratorValidator : AbstractValidator<Administrator>
    {
        public AdministratorValidator()
        {
            RuleFor(x => x.Surname)
                .NotEmpty().WithMessage("Фамилия не может быть пустой")
                .Length(1, 50).WithMessage("Фамилия должна быть от 1 до 50 символов")
                .Matches(@"^[а-яА-ЯёЁa-zA-Z\-]+$").WithMessage("Фамилия может содержать только буквы и дефис");
            
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Имя не может быть пустым")
                .Length(1, 50).WithMessage("Имя должно быть от 1 до 50 символов")
                .Matches(@"^[а-яА-ЯёЁa-zA-Z\-]+$").WithMessage("Имя может содержать только буквы и дефис");
            
            RuleFor(x => x.Patronymic)
                .Length(0, 50).When(x => !string.IsNullOrEmpty(x.Patronymic))
                .WithMessage("Отчество должно быть до 50 символов")
                .Matches(@"^[а-яА-ЯёЁa-zA-Z\-]*$").When(x => !string.IsNullOrEmpty(x.Patronymic))
                .WithMessage("Отчество может содержать только буквы и дефис");
            
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Номер телефона не может быть пустым")
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Номер телефона должен быть в международном формате")
                .MinimumLength(10).WithMessage("Номер телефона должен содержать минимум 10 цифр")
                .MaximumLength(15).WithMessage("Номер телефона должен содержать максимум 15 цифр")
                .Must(BeAValidPhoneNumber).WithMessage("Номер телефона имеет неверный формат");
        }

        private bool BeAValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            // Убираем все нецифровые символы кроме +
            var digitsOnly = phoneNumber.StartsWith('+') 
                ? "+" + new string(phoneNumber.Substring(1).Where(char.IsDigit).ToArray())
                : new string(phoneNumber.Where(char.IsDigit).ToArray());

            // Проверяем базовые требования
            if (digitsOnly.StartsWith('+'))
            {
                return digitsOnly.Length >= 11 && digitsOnly.Length <= 16;
            }
            else
            {
                return digitsOnly.Length >= 10 && digitsOnly.Length <= 15;
            }
        }
    }
}