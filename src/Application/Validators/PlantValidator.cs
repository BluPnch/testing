using Domain.Models;
using FluentValidation;


namespace Application.Validators
{
    public class PlantValidator : AbstractValidator<Plant>
    {
        public PlantValidator()
        {
            RuleFor(x => x.Specie)
                .NotEmpty().WithMessage("Вид растения не может быть пустым.")
                .MaximumLength(100).WithMessage("Длина вида растения не должна превышать 100 символов.");

            RuleFor(x => x.Family)
                .NotEmpty().WithMessage("Семейство растения не может быть пустым.")
                .MaximumLength(100).WithMessage("Длина семейства растения не должна превышать 100 символов.");

            RuleFor(x => x.Flower)
                .IsInEnum().WithMessage("Неверное значение для цветка растения.");

            RuleFor(x => x.Fruit)
                .IsInEnum().WithMessage("Неверное значение для плода растения.");

            RuleFor(x => x.Reproduction)
                .IsInEnum().WithMessage("Неверное значение для способа размножения растения.");
        }
    }
}