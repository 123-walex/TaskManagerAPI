using FluentValidation;
using FluentValidation.AspNetCore;
using TaskManagerAPI.DTO_s;

namespace TaskManagerAPI.Validators
{
    public class TotalUpdateValidator : AbstractValidator<TotalUpdateUserDTO>
    {
        public TotalUpdateValidator() 
        {
            RuleFor(x => x.NewEmail).NotEmpty().WithMessage("Requires new email");
            RuleFor(x => x.OldEmail).NotEmpty().WithMessage("Requires old email");
            RuleFor(x => x.NewPassword).NotEmpty().WithMessage("Requires New Password");
            RuleFor(x => x.OldPassword).NotEmpty().WithMessage("Requires Old Password");
        }
    }
}
