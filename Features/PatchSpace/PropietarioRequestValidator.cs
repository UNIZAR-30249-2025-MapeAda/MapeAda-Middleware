using FluentValidation;
using MapeAda_Middleware.SharedModels.Spaces;

namespace MapeAda_Middleware.Features.PatchSpace;

public class PropietarioRequestValidator : AbstractValidator<PropietarioRequest>
{
    public PropietarioRequestValidator()
    {
        RuleFor(p => p.Tipo)
            .Must(uso => Enum.IsDefined(typeof(TipoPropietario), uso))
            .WithMessage("El tipo de propietario no es válido.");

        RuleFor(p => p.Id)
            .NotEmpty().WithMessage("El identificador del propietario no puede estar vacío.");
    }
}
