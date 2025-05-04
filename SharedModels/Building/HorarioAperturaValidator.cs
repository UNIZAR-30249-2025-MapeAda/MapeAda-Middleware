using FluentValidation;

namespace MapeAda_Middleware.SharedModels.Building;

public class HorarioAperturaValidator : AbstractValidator<HorarioApertura>
{
    public HorarioAperturaValidator()
    {
        RuleFor(x => x.Fecha)
            .NotEmpty()
            .WithMessage("La fecha de apertura es obligatoria.");

        When(x => x.Intervalo != null, () =>
        {
            RuleFor(x => x.Intervalo)
                .SetValidator(new IntervaloValidator()!);
        });
    }
}