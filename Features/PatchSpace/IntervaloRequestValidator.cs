using FluentValidation;

namespace MapeAda_Middleware.Features.PatchSpace;

public class IntervaloRequestValidator : AbstractValidator<IntervaloRequest>
{
    public IntervaloRequestValidator()
    {
        RuleFor(x => x.Inicio)
            .NotEmpty().WithMessage("La hora de inicio no puede estar vacía.")
            .LessThan(x => x.Fin).WithMessage("La hora de inicio debe ser anterior a la hora de fin.");

        RuleFor(x => x.Fin)
            .NotEmpty().WithMessage("La hora de fin no puede estar vacía.");
    }
}
