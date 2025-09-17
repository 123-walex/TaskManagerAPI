using FluentValidation;
using TaskManagerAPI.DTO_s;

namespace TaskManagerAPI.Validators
{
    public class UpdateTaskValidator : AbstractValidator<UpdateTask>
    {
        public UpdateTaskValidator()
        {
            RuleFor(x => x.Title).NotEmpty().WithMessage("Title is a required field");
            RuleFor(x => x.Description).NotEmpty().WithMessage("Description is a required field");
            RuleFor(x => x.DueTime).NotNull().WithMessage("DueTime Is required");
            RuleFor(x => x.DueDate).NotNull().WithMessage("DueDate is required.");
        }
    }
}
