using FluentValidation;
using MapeAda_Middleware.SharedModels.Users;

namespace MapeAda_Middleware.Features.PatchUser;

public class PatchUserRequestValidator : AbstractValidator<PatchUserRequest>
{
    public PatchUserRequestValidator()
    {
        RuleFor(x => x.Rol)
            .Must(Enum.IsDefined)
            .WithMessage("El rol no es válido.");

        RuleFor(x => x.Departamento)
            .Must(dept => dept == null || Enum.IsDefined(typeof(Departamento), dept))
            .WithMessage("El departamento no es válido.");
    }
}