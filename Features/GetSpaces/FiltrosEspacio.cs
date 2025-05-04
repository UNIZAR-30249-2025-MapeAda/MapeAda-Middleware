using MapeAda_Middleware.SharedModels.Spaces;

namespace MapeAda_Middleware.Features.GetSpaces;

public sealed record FiltrosEspacio(string? Nombre, TipoEspacio? Categoria, int? CapacidadMaxima, string? Planta);