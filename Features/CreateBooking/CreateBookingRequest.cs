using MapeAda_Middleware.SharedModels.Bookings;

namespace MapeAda_Middleware.Features.CreateBooking;

public sealed record CreateBookingRequest(
    IEnumerable<string> Espacios,
    TipoUso Uso,
    int Asistentes,
    Periodo Periodo,
    string? Observaciones)
{
    public CreateBookingBackendRequest ToBackendRequest(string usuarioNip)
    {
        return new CreateBookingBackendRequest(usuarioNip, Espacios, Uso, Asistentes, Periodo, Observaciones);
    }
};
