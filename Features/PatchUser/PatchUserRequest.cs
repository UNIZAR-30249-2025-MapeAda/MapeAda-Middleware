using MapeAda_Middleware.SharedModels.Users;

namespace MapeAda_Middleware.Features.PatchUser;

public sealed class PatchUserRequest
{
    public PatchUserRequest(Rol rol, Departamento? departamento)
    {
        Rol = rol;
        Departamento = departamento;
    }

    internal PatchUserRequest()
    {
    }

    public Rol Rol { get; init; }
    public Departamento? Departamento { get; init; }
};