using FluentValidation;

namespace MapeAda_Middleware.SharedModels.Bookings;

public class PeriodoValidator : AbstractValidator<Periodo>
{
    public PeriodoValidator()
    {
        CascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Inicio)
            .NotEmpty().WithMessage("La fecha de inicio no puede estar vacía.");

        RuleFor(x => x.Fin)
            .NotEmpty().WithMessage("La fecha de fin no puede estar vacía.");

        RuleFor(x => x)
            .Must(p => p.Inicio < p.Fin)
            .WithMessage("La fecha de inicio debe ser anterior a la fecha de fin.");

        RuleFor(x => x)
            .Must(p => p.Inicio.Date == p.Fin.Date)
            .WithMessage("El periodo debe iniciarse y finalizar el mismo día.");
    }
}
