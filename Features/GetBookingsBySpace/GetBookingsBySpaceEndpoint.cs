using MapeAda_Middleware.Abstract;
using MapeAda_Middleware.Extensions;
using MapeAda_Middleware.SharedModels.Bookings;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MapeAda_Middleware.Features.GetBookingsBySpace;

public class GetBookingsBySpaceEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("/api/bookings/space/{id}", Handle)
            .RequireAuthorization()
            .WithMetadata(new SwaggerOperationAttribute("Obtiene reservas por espacio"))
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status200OK,
                "Reservas del espacio",
                typeof(IEnumerable<Reserva>)))
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
            .WithTags("Reservas");
    }

    private static async Task<IResult> Handle(
        [FromRoute][SwaggerParameter("Id del espacio", Required = true)] string id,
        IHttpClientFactory httpClientFactory)
    {
        HttpClient client = httpClientFactory.CreateClient(Constants.BackendHttpClientName);

        HttpResponseMessage response = await client.GetAsync($"api/bookings/space/{id}");

        if (!response.IsSuccessStatusCode)
        {
            return await response.ToProblem();
        }

        IEnumerable<Reserva> bookings = await response.Content.ReadFromJsonAsync<IEnumerable<Reserva>>();

        return Results.Ok(bookings!);
    }
}
