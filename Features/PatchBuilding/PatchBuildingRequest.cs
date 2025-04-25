namespace MapeAda_Middleware.Features.PatchBuilding;

public class PatchBuildingRequest
{
    public PatchBuildingRequest(decimal usoMaximo, List<HorariosAperturaEdificioRequest> horariosApertura)
    {
        UsoMaximo = usoMaximo;
        HorariosApertura = horariosApertura;
    }

    internal PatchBuildingRequest()
    {
    }

    public decimal UsoMaximo { get; init; }
    public List<HorariosAperturaEdificioRequest> HorariosApertura { get; init; }
}
