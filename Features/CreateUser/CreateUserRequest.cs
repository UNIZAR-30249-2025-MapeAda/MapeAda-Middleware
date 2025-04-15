using MapeAda_Middleware.SharedModels.Users;

namespace MapeAda_Middleware.Features.CreateUser;

public sealed record CreateUserRequest(
    string Nombre,
    string Apellidos,
    string Telefono,
    string Nip,
    string Email,
    Rol Rol,
    Departamento? Departamento);
