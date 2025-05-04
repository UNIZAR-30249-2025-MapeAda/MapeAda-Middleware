using FluentValidation;

namespace MapeAda_Middleware.SharedModels;

public class IntervaloValidator : AbstractValidator<Intervalo>
{
    public IntervaloValidator()
    {
        RuleFor(x => x.Inicio)
            .NotEmpty().WithMessage("La hora de inicio no puede estar vacía.")
            .LessThan(x => x.Fin).WithMessage("La hora de inicio debe ser anterior a la hora de fin.");

        RuleFor(x => x.Fin)
            .NotEmpty().WithMessage("La hora de fin no puede estar vacía.");
    }
}
