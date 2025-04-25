using FluentValidation;
using MapeAda_Middleware.SharedModels.Bookings;

namespace MapeAda_Middleware.Features.CreateBooking;

public class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(x => x.Espacios)
            .NotNull().WithMessage("Debe especificar al menos un espacio.")
            .Must(list => list.Any()).WithMessage("Debe especificar al menos un espacio.")
            .Must(list => list.All(id => !string.IsNullOrWhiteSpace(id)))
            .WithMessage("Los identificadores de espacio no pueden estar vacíos.")
            .Must(list => list.Distinct().Count() == list.Count())
            .WithMessage("No se permiten espacios duplicados.");

        RuleFor(x => x.Uso)
            .Must(uso => Enum.IsDefined(typeof(TipoUso), uso))
            .WithMessage("El tipo de uso no es válido.");

        RuleFor(x => x.Asistentes)
            .GreaterThan(0).WithMessage("El número de asistentes debe ser mayor que cero.");

        RuleFor(x => x.Periodo)
            .NotNull().WithMessage("El periodo no puede ser nulo.")
            .SetValidator(new PeriodoRequestValidator());

        RuleFor(x => x.Observaciones)
            .MaximumLength(500).WithMessage("Las observaciones no pueden exceder los 500 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Observaciones));
    }
}
