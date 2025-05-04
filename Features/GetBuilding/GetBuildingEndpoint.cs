using MapeAda_Middleware.Abstract;
using MapeAda_Middleware.Extensions;
using MapeAda_Middleware.SharedModels.Building;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MapeAda_Middleware.Features.GetBuilding;

public class GetBuildingEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("/api/building", Handle)
            .RequireAuthorization()
            .WithMetadata(new SwaggerOperationAttribute("Obtiene los datos del edificio"))
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status200OK,
                "Configuración del edificio obtenida",
                typeof(Edificio)))
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status401Unauthorized,
                "Necesitas iniciar sesión",
                typeof(ProblemDetails)))
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status500InternalServerError,
                "Error no controlado",
                typeof(ProblemDetails)))
            .WithTags("Edificio");
    }

    private static async Task<IResult> Handle(
        IHttpClientFactory httpClientFactory)
    {
        HttpClient client = httpClientFactory.CreateClient(Constants.BackendHttpClientName);

        HttpResponseMessage response = await client.GetAsync("api/building");

        if (!response.IsSuccessStatusCode)
        {
            return await response.ToProblem();
        }

        Edificio edificio = await response.Content.ReadFromJsonAsync<Edificio>() ?? throw new InvalidOperationException("Ha ocurrido un error al deserializar el cuerpo de la petición");

        return Results.Ok(edificio!);
    }
}
