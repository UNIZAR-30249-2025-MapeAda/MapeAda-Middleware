using FluentValidation;

namespace MapeAda_Middleware.SharedModels.Building;

public class EdificioValidator : AbstractValidator<Edificio>
{
    public EdificioValidator()
    {
        RuleFor(x => x.PorcentajeUsoMaximo)
            .NotNull().WithMessage("El porcentaje de uso máximo no puede ser nulo.")
            .SetValidator(new PorcentajeValidator());

        RuleFor(x => x.CalendarioApertura)
            .NotNull().WithMessage("El calendario de apertura no puede ser nulo.")
            .SetValidator(new CalendarioValidator());
    }
}