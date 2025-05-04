namespace MapeAda_Middleware.SharedModels.Bookings;

public sealed record Reserva(
    long Id,
    IEnumerable<string> Espacios,
    string Usuario,
    TipoUso Uso,
    int Asistentes,
    Periodo Periodo,
    string? Observaciones,
    bool Valida,
    DateTime? InvalidSince,
    DateTime? DeletedAt);
