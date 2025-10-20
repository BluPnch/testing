using Domain.Models;
using FluentValidation;

namespace Application.Validators
{
    public class JournalRecordValidator : AbstractValidator<JournalRecord>
    {
        public JournalRecordValidator()
        {
            RuleFor(x => x.PlantId)
                .NotEmpty().WithMessage("Идентификатор растения обязателен");

            RuleFor(x => x.GrowthStageId)
                .NotEmpty().WithMessage("Идентификатор этапа роста обязателен");

            RuleFor(x => x.EmployeeId)
                .NotEmpty().WithMessage("Идентификатор сотрудника обязателен");

            RuleFor(x => x.PlantHeight)
                .GreaterThan(0).WithMessage("Высота растения должна быть положительным числом")
                .LessThanOrEqualTo(1000).WithMessage("Высота растения не может превышать 1000 см");
                
            RuleFor(x => x.FruitCount)
                .GreaterThanOrEqualTo(0).WithMessage("Количество плодов не может быть отрицательным")
                .LessThanOrEqualTo(10000).WithMessage("Количество плодов не может превышать 10000");
                
            RuleFor(x => x.Condition)
                .IsInEnum().WithMessage("Указано недопустимое значение состояния растения");
            
            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("Дата записи обязательна")
                .LessThanOrEqualTo(DateTimeOffset.Now.Add(new TimeSpan(1, 0, 0)))
                .WithMessage("Дата записи не может быть в будущем")
                .GreaterThanOrEqualTo(new DateTimeOffset(2015, 1, 1, 0, 0, 0, TimeSpan.Zero))
                .WithMessage("Дата записи не может быть раньше 2015 года");
        }
    }
}