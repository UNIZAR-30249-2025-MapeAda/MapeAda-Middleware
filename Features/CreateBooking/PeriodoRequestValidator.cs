using FluentValidation;

namespace MapeAda_Middleware.Features.CreateBooking;

public class PeriodoRequestValidator : AbstractValidator<PeriodoRequest>
{
    public PeriodoRequestValidator()
    {
        RuleFor(x => x.Inicio)
            .NotEmpty().WithMessage("La fecha de inicio no puede estar vacía.")
            .LessThan(x => x.Fin).WithMessage("La fecha de inicio debe ser anterior a la fecha de fin.");

        RuleFor(x => x.Fin)
            .NotEmpty().WithMessage("La fecha de fin no puede estar vacía.");
    }
}
