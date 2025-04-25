using FluentValidation;
using MapeAda_Middleware.SharedModels.Spaces;

namespace MapeAda_Middleware.Features.PatchSpace;

public class PatchSpaceRequestValidator : AbstractValidator<PatchSpaceRequest>
{
    public PatchSpaceRequestValidator()
    {
        RuleFor(x => x.Categoria)
            .Must(dept => Enum.IsDefined(typeof(TipoEspacio), dept))
            .WithMessage("La categoría no es válida.");

        RuleFor(x => x.Horario)
            .NotNull().WithMessage("El horario no puede ser nulo.")
            .SetValidator(new IntervaloRequestValidator());

        RuleFor(x => x.Propietarios)
            .NotNull().WithMessage("La lista de propietarios no puede ser nula.")
            .Must(list => list.Any())
            .WithMessage("Debe existir al menos un propietario.")
            .DependentRules(() =>
            {
                RuleForEach(x => x.Propietarios)
                    .SetValidator(new PropietarioRequestValidator());
            });
    }
}
