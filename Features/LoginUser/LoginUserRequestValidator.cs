using FluentValidation;

namespace MapeAda_Middleware.Features.LoginUser;

public class LoginUserRequestValidator: AbstractValidator<LoginUserRequest>
{
    public LoginUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
            .EmailAddress().WithMessage("El formato del correo electrónico no es válido.");
    }
}