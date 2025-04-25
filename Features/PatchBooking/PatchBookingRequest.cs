namespace MapeAda_Middleware.Features.PatchBooking;

public class PatchBookingRequest
{
    public PatchBookingRequest(bool valida)
    {
        Valida = valida;
    }

    internal PatchBookingRequest()
    {
    }

    public bool Valida { get; init; }
}
