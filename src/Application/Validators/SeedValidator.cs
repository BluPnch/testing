using Domain.Models;
using FluentValidation;

namespace Application.Validators
{
    public class SeedValidator : AbstractValidator<Seed>
    {
        public SeedValidator()
        {
            RuleFor(x => x.PlantId)
                .NotEmpty().WithMessage("ID растения не может быть пустым");
                
            RuleFor(x => x.Maturity)
                .NotEmpty().WithMessage("Стадия зрелости не может быть пустой")
                .MaximumLength(100).WithMessage("Длина стадии зрелости не должна превышать 100 символов");
                
            RuleFor(x => x.Viability)
                .IsInEnum().WithMessage("Неверное значение жизнеспособности семени");
                
            RuleFor(x => x.LightRequirements)
                .IsInEnum().WithMessage("Неверное значение освещения семени");
                
            RuleFor(x => x.WaterRequirements)
                .NotEmpty().WithMessage("Требования к поливу не могут быть пустыми")
                .MaximumLength(200).WithMessage("Длина требований к поливу не должна превышать 200 символов");
                
            RuleFor(x => x.TemperatureRequirements)
                .InclusiveBetween(-30, 40)
                .WithMessage("Температура должна быть в пределах от -30 до 40 градусов");
        }
    }
}