using FluentValidation;

namespace MapeAda_Middleware.SharedModels.Spaces;

public class PropietarioValidator : AbstractValidator<Propietario>
{
    public PropietarioValidator()
    {
        RuleFor(p => p.Tipo)
            .Must(uso => Enum.IsDefined(typeof(TipoPropietario), uso))
            .WithMessage("El tipo de propietario no es válido.");

        RuleFor(p => p.Id)
            .NotEmpty().WithMessage("El identificador del propietario no puede estar vacío.");
    }
}
