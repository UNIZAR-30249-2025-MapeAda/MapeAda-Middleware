using MapeAda_Middleware.SharedModels.Spaces;

namespace MapeAda_Middleware.Features.PatchSpace;

public class PatchSpaceRequest
{
    public PatchSpaceRequest(bool reservable, TipoEspacio categoria, IntervaloRequest horario, IEnumerable<PropietarioRequest> propietarios)
    {
        Reservable = reservable;
        Categoria = categoria;
        Horario = horario;
        Propietarios = propietarios;
    }

    internal PatchSpaceRequest()
    {
    }

    public bool Reservable { get; init; }
    public TipoEspacio Categoria { get; init; }
    public IntervaloRequest Horario { get; init; }
    public IEnumerable<PropietarioRequest> Propietarios { get; init; }
}
