namespace MapeAda_Middleware.SharedModels.Users;

public sealed record Usuario(
    string Nombre,
    string Apellidos,
    string Telefono,
    string Nip,
    string Email,
    Rol Rol,
    Departamento? Departamento);
