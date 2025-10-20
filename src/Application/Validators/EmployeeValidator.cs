using Domain.Models;
using FluentValidation;


namespace Application.Validators
{
    public class EmployeeValidator : AbstractValidator<Employee>
    {
        public EmployeeValidator()
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
            
            RuleFor(x => x.Task)
                .NotEmpty().WithMessage("Задача не может быть пустой")
                .Length(5, 500).WithMessage("Задача должна быть от 5 до 500 символов");
            
            RuleFor(x => x.PlantDomain)
                .NotEmpty().WithMessage("Сфера растений не может быть пустой")
                .Length(3, 100).WithMessage("Сфера растений должна быть от 3 до 100 символов");
            
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Номер телефона не может быть пустой")
                .Matches(@"^\+?[0-9\s\-\(\)]{7,20}$")
                .WithMessage("Номер телефона должен быть в международном или местном формате");
        }
    }
}