using FluentValidation;
using TaskManagerAPI.DTO_s;

namespace TaskManagerAPI.Validators
{
    public class AuthResponseValidator : AbstractValidator<AuthResponse>
    {
        public AuthResponseValidator() 
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required");
            RuleFor(x => x.AccessToken).NotEmpty().WithMessage("Accesstoken required");
            RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("Refreshtoken is required");
            RuleFor(x => x.Role).NotNull().WithMessage("Role claim is required");
        }
    }
}
