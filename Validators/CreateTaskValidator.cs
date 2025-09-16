using FluentValidation;
using TaskManagerAPI.DTO_s;

namespace TaskManagerAPI.Validators
{
    public class CreateTaskValidator : AbstractValidator<CreateTask>
    {
        public CreateTaskValidator() 
        {
            RuleFor(x => x.Title).NotEmpty().WithMessage("Title is empty");
            RuleFor(x => x.Description).NotEmpty().WithMessage("Description is empty");
            RuleFor(x => x.Duetime).NotNull().WithMessage("Duetime is empty");
            RuleFor(x => x.DueDate).NotNull().WithMessage("Duedate is empty");
        }

    }
}
