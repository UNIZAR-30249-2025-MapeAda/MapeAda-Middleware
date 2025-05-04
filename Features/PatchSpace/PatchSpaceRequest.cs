using MapeAda_Middleware.SharedModels;
using MapeAda_Middleware.SharedModels.Spaces;

namespace MapeAda_Middleware.Features.PatchSpace;

public class PatchSpaceRequest
{
    public PatchSpaceRequest(bool reservable, TipoEspacio categoria, Intervalo? horario, IEnumerable<Propietario> propietarios)
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
    public Intervalo? Horario { get; init; }
    public IEnumerable<Propietario> Propietarios { get; init; }
}
