using MapeAda_Middleware.Abstract;
using MapeAda_Middleware.Extensions;
using MapeAda_Middleware.SharedModels.Bookings;
using MapeAda_Middleware.SharedModels.Spaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MapeAda_Middleware.Features.GetSpaceById;

public class GetSpaceById : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("/api/spaces/{id}", Handle)
            .RequireAuthorization(Constants.GerenteOnlyPolicyName)
            .WithMetadata(new SwaggerOperationAttribute("Obtiene la reserva por el id"))
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status200OK,
                "Espacio por id",
                typeof(Espacio)))
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status401Unauthorized,
                "Necesitas iniciar sesión",
                typeof(ProblemDetails)))
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status403Forbidden,
                "No tienes permisos suficientes",
                typeof(ProblemDetails)))
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status404NotFound,
                "Espacio no encontrado",
                typeof(ProblemDetails)))
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status500InternalServerError,
                "Error no controlado",
                typeof(ProblemDetails)))
            .WithTags("Espacios");
    }

    private static async Task<IResult> Handle(
        [FromRoute][SwaggerParameter("Id del espacio", Required = true)] string id,
        IHttpClientFactory httpClientFactory)
    {
        HttpClient client = httpClientFactory.CreateClient(Constants.BackendHttpClientName);

        HttpResponseMessage response = await client.GetAsync($"api/spaces/{id}");

        if (!response.IsSuccessStatusCode)
        {
            return await response.ToProblem();
        }

        Espacio space = await response.Content.ReadFromJsonAsync<Espacio>() ?? throw new InvalidOperationException("Ha ocurrido un error al deserializar el cuerpo de la petición");

        return Results.Ok(space);
    }
}
