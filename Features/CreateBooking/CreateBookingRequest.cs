using MapeAda_Middleware.SharedModels.Bookings;

namespace MapeAda_Middleware.Features.CreateBooking;

public sealed record CreateBookingRequest(
    IEnumerable<string> Espacios,
    TipoUso Uso,
    int Asistentes,
    PeriodoRequest Periodo,
    string? Observaciones);
