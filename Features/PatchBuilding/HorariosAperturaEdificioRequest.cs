namespace MapeAda_Middleware.Features.PatchBuilding;

public sealed record HorariosAperturaEdificioRequest(string diaSemana, TimeOnly horaApertura, TimeOnly horaCierre);
