using FluentValidation;
using TaskManagerAPI.DTO_s;

namespace TaskManagerAPI.Validators
{
    public class GoogleLoginValidators : AbstractValidator<LoginDTO_Google_>
    {
        public GoogleLoginValidators() 
        {
            RuleFor(x => x.IDToken).NotEmpty().WithMessage("Requires Id token");
            RuleFor(x => x.Role).NotNull().WithMessage("Requires specifed role");
        }
    }
}
