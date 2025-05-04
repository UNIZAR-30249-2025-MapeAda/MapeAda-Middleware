using FluentValidation;
using MapeAda_Middleware.SharedModels;
using MapeAda_Middleware.SharedModels.Spaces;

namespace MapeAda_Middleware.Features.PatchSpace;

public class PatchSpaceRequestValidator : AbstractValidator<PatchSpaceRequest>
{
    public PatchSpaceRequestValidator()
    {
        RuleFor(x => x.Categoria)
            .IsInEnum()
            .WithMessage("La categoría no es válida.");

        When(x => x.Horario != null, () =>
        {
            RuleFor(x => x.Horario)
                .SetValidator(new IntervaloValidator()!);
        });

        RuleFor(x => x.Propietarios)
            .NotNull().WithMessage("La lista de propietarios no puede ser nula.")
            .Must(list => list.Any())
            .WithMessage("Debe existir al menos un propietario.")
            .Must(list => list.Select(p => p.Id).Distinct().Count() == list.Count())
            .WithMessage("No puede haber propietarios duplicados.")
            .DependentRules(() =>
            {
                RuleForEach(x => x.Propietarios)
                    .SetValidator(new PropietarioValidator());
            });
    }
}
