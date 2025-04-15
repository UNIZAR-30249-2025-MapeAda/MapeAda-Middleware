using FluentValidation;
using MapeAda_Middleware.SharedModels.Users;

namespace MapeAda_Middleware.Features.CreateUser;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre no puede estar vacío.")
            .Matches(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ\s]+$")
            .WithMessage("El nombre solo puede contener letras y espacios.");

        RuleFor(x => x.Apellidos)
            .NotEmpty().WithMessage("Los apellidos no pueden estar vacíos.")
            .Matches(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ\s]+$")
            .WithMessage("Los apellidos solo pueden contener letras y espacios.");

        RuleFor(x => x.Telefono)
            .NotEmpty().WithMessage("El teléfono no puede estar vacío.")
            .Matches(@"^\d{9}$")
            .WithMessage("El teléfono debe contener exactamente 9 dígitos numéricos.");

        RuleFor(x => x.Nip)
            .NotEmpty().WithMessage("El NIP no puede estar vacío.")
            .Matches(@"^\d{6}$")
            .WithMessage("El NIP debe contener exactamente 6 dígitos numéricos.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email no puede estar vacío.")
            .EmailAddress().WithMessage("El email no es válido.");

        RuleFor(x => x.Rol)
            .Must(Enum.IsDefined)
            .WithMessage("El rol no es válido.");

        RuleFor(x => x.Departamento)
            .Must(dept => dept == null || Enum.IsDefined(typeof(Departamento), dept))
            .WithMessage("El departamento no es válido.");
    }
}
