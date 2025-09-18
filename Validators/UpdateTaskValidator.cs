using FluentValidation;
using TaskManagerAPI.DTO_s;

namespace TaskManagerAPI.Validators
{
    public class UpdateTaskValidator : AbstractValidator<TotalUpdateTaskDTO>
    {
        public UpdateTaskValidator()
        {
            RuleFor(x => x.NewTitle).NotEmpty().WithMessage("NewTitle is a required field");
            RuleFor(x => x.NewDescription).NotEmpty().WithMessage("NewDescription is a required field");
            RuleFor(x => x.NewDueTime).NotNull().WithMessage("NewDueTime Is required");
            RuleFor(x => x.NewDueDate).NotNull().WithMessage("NewDueDate is required.");
            RuleFor(x => x.OldTitle).NotEmpty().WithMessage("OldTitle is a required field");
            RuleFor(x => x.OldDescription).NotEmpty().WithMessage("OldDescription is a required field");
            RuleFor(x => x.OldDueTime).NotNull().WithMessage("OldDueTime Is required");
            RuleFor(x => x.OldDueDate).NotNull().WithMessage("OldDueDate is required.");
        }
    }
}
