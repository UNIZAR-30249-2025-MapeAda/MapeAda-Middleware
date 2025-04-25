using FluentValidation;

namespace MapeAda_Middleware.Features.PatchBuilding;

public class PatchBuildingRequestValidator : AbstractValidator<PatchBuildingRequest>
{
    public PatchBuildingRequestValidator()
    {
        RuleFor(x => x.UsoMaximo)
            .GreaterThan(0m).WithMessage("El uso máximo debe ser mayor que 0.");

        RuleFor(x => x.HorariosApertura)
            .NotNull().WithMessage("La lista de horarios de apertura no puede ser nula.")
            .Must(list => list.Any())
            .WithMessage("Debe existir al menos un horario de apertura.")
            .DependentRules(() =>
            {
                RuleForEach(x => x.HorariosApertura)
                    .SetValidator(new HorariosAperturaEdificioRequestValidator());
            });
    }
}
