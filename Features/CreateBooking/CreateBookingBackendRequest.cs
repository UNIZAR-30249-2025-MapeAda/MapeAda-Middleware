using MapeAda_Middleware.SharedModels.Bookings;

namespace MapeAda_Middleware.Features.CreateBooking;

public record CreateBookingBackendRequest(string Nip,
    IEnumerable<string> Espacios,
    TipoUso Uso,
    int Asistentes,
    Periodo Periodo,
    string? Observaciones);