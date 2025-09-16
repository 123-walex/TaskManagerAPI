using FluentValidation;
using TaskManagerAPI.DTO_s;

namespace TaskManagerAPI.Validators
{
    public class ManualLoginValidator : AbstractValidator<LoginDTO_Manual_>
    {
        public ManualLoginValidator()
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required");
            RuleFor(x => x.Role).NotNull().WithMessage("Role Is required"); 
        }
    }
}
