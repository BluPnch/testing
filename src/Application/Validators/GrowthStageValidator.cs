using Domain.Models;
using FluentValidation;


namespace Application.Validators
{
    public class GrowthStageValidator : AbstractValidator<GrowthStage>
    {
        public GrowthStageValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Этап роста не может быть пустым.")
                .MaximumLength(100).WithMessage("Длина этапа роста не должна превышать 100 символов.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Описание этапа роста не может быть пустым.")
                .MaximumLength(1000).WithMessage("Длина этапа роста не должна превышать 1000 символов.");
        }
    }
}