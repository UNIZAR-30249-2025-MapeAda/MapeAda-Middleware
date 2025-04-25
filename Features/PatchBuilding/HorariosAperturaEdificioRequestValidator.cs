using FluentValidation;

namespace MapeAda_Middleware.Features.PatchBuilding;

public class HorariosAperturaEdificioRequestValidator : AbstractValidator<HorariosAperturaEdificioRequest>
{
    private static readonly string[] DiasValidos = new[]
    {
        "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo"
    };

    public HorariosAperturaEdificioRequestValidator()
    {
        RuleFor(x => x.diaSemana)
            .NotEmpty().WithMessage("El día de la semana es obligatorio.")
            .Must(d => DiasValidos.Any(v => v.Equals(d, StringComparison.OrdinalIgnoreCase)))
            .WithMessage("El día de la semana '{PropertyValue}' no es válido. Debe ser uno de: "
                    + string.Join(", ", DiasValidos) + ".");

        RuleFor(x => x.horaApertura)
            .NotEmpty().WithMessage("La hora de apertura no puede estar vacía.")
            .LessThan(x => x.horaCierre)
            .WithMessage("La hora de apertura debe ser anterior a la hora de cierre.");

        RuleFor(x => x.horaCierre)
            .NotEmpty().WithMessage("La hora de cierre no puede estar vacía.");            
    }
}
