namespace MapeAda_Middleware.SharedModels.Spaces;

public sealed record Espacio(
    string Id,
    float Tamanyo,
    string Nombre,
    TipoEspacio Tipo,
    int Capacidad,
    int Planta,
    bool Reservable,
    TipoEspacio Categoria,
    Intervalo? Horario,
    IEnumerable<Propietario> Propietarios);
