using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Swashbuckle.AspNetCore.Annotations;
using MapeAda_Middleware.Abstract;
using MapeAda_Middleware.Extensions;
using MapeAda_Middleware.SharedModels.Spaces;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
                   
namespace MapeAda_Middleware.Features.GetSpaces;

public class GetSpacesEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("/api/spaces", Handle)
            .AddFluentValidationAutoValidation()
            .RequireAuthorization()
            .WithMetadata(new SwaggerOperationAttribute("Busca espacios con filtros"))
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status200OK,
                "Lista de espacios encontrados",
                typeof(IEnumerable<Espacio>)))
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status400BadRequest,
                "Datos de filtrado inválidos",
                typeof(ProblemDetails)))
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status500InternalServerError,
                "Error no controlado",
                typeof(ProblemDetails)))
            .WithTags("Espacios");
    }

    private static async Task<IResult> Handle(
        [AsParameters] FiltrosEspacio filtros,
        IHttpClientFactory httpClientFactory)
    {
        HttpClient client = httpClientFactory.CreateClient(Constants.BackendHttpClientName);

        Dictionary<string, string?> queryParams = new Dictionary<string, string?>();

        if (!string.IsNullOrEmpty(filtros.Nombre))
        {
            queryParams["Nombre"] = filtros.Nombre;
        }

        if (filtros.Categoria is not null)
        {
            queryParams["Categoria"] = filtros.Categoria.ToString();
        }

        if (filtros.CapacidadMaxima is not null)
        {
            queryParams["CapacidadMaxima"] = filtros.CapacidadMaxima.Value.ToString();
        }

        if (!string.IsNullOrEmpty(filtros.Planta))
        {
            queryParams["Planta"] = filtros.Planta;
        }

        string url = queryParams.Count != 0
            ? QueryHelpers.AddQueryString("api/spaces", queryParams)
            : "api/spaces";

        HttpResponseMessage response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            return await response.ToProblem();
        }

        IEnumerable<Espacio> espacios = await response.Content.ReadFromJsonAsync<IEnumerable<Espacio>>() ?? throw new InvalidOperationException("Ha ocurrido un error al deserializar el cuerpo de la petición");
        
        return Results.Ok(espacios);
    }
}
