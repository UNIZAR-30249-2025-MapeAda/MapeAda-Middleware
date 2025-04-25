using MapeAda_Middleware.Features.CreateBooking;
using MapeAda_Middleware.SharedModels.Spaces;

namespace MapeAda_Middleware.SharedModels.Bookings;

public sealed record Reserva(
    long Id,
    IEnumerable<EspacioInfo> Espacios,
    string Usuario,
    TipoUso Uso,
    int Asistentes,
    PeriodoRequest Periodo,
    string? Observaciones,
    bool Valida);
