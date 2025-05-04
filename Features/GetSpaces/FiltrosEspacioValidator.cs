using FluentValidation;
using MapeAda_Middleware.SharedModels.Spaces;

namespace MapeAda_Middleware.Features.GetSpaces;

public class FiltrosEspacioValidator : AbstractValidator<FiltrosEspacio>
{
    private static readonly string[] PlantasValidas = ["0", "1", "2", "3", "4"];

    public FiltrosEspacioValidator()
    {
        RuleFor(x => x.Categoria)
            .Must(cat => cat == null || Enum.IsDefined(typeof(TipoEspacio), cat))
            .WithMessage("La categoría no es un valor válido.");

        RuleFor(x => x.CapacidadMaxima)
            .GreaterThan(0)
            .When(x => x.CapacidadMaxima is not null)
            .WithMessage("La capacidad máxima debe ser un número mayor que cero.");

        RuleFor(x => x.Planta)
            .Must(p => string.IsNullOrEmpty(p) || PlantasValidas.Contains(p))
            .WithMessage("La planta sólo puede ser 0, 1, 2, 3 o 4.");
    }
}