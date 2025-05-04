using FluentValidation;

namespace MapeAda_Middleware.SharedModels.Building;

public class PorcentajeValidator : AbstractValidator<Porcentaje>
{
    public PorcentajeValidator()
    {
        RuleFor(x => x.Valor)
            .InclusiveBetween(0f, 100f)
            .WithMessage("El porcentaje debe estar entre 0 y 100.");
    }
}