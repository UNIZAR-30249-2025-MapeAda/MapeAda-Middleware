using FluentValidation;

namespace MapeAda_Middleware.SharedModels.Building;

public class DiasValidator : AbstractValidator<Dias>
{
    private const Dias AllDays = 
        Dias.Lunes 
        | Dias.Martes 
        | Dias.Miercoles 
        | Dias.Jueves 
        | Dias.Viernes 
        | Dias.Sabado 
        | Dias.Domingo;

    public DiasValidator()
    {
        RuleFor(x => x)
            .Must(value => (value & ~AllDays) == 0)
            .WithMessage("El valor de 'Dias' contiene flags no válidos.");
    }
}