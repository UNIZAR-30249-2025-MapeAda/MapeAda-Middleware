using FluentValidation;

namespace MapeAda_Middleware.SharedModels.Building;

public class CalendarioValidator : AbstractValidator<Calendario>
{
    public CalendarioValidator()
    {
        RuleFor(x => x.HorariosApertura)
            .NotNull().WithMessage("La lista de horarios de apertura no puede ser nula.");

        When(x => x.HorariosApertura != null && x.HorariosApertura.Any(), () =>
        {
            RuleForEach(x => x.HorariosApertura)
                .SetValidator(new HorarioAperturaValidator());
        });

        RuleFor(x => x.IntervaloPorDefecto)
            .NotNull().WithMessage("El intervalo por defecto no puede ser nulo.")
            .SetValidator(new IntervaloValidator());

        RuleFor(x => x.DiasPorDefecto)
            .NotNull().WithMessage("Los días por defecto no pueden ser nulos.")
            .SetValidator(new DiasValidator());
    }
}