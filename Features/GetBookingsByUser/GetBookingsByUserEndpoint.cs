using MapeAda_Middleware.Abstract;
using MapeAda_Middleware.Extensions;
using MapeAda_Middleware.SharedModels.Bookings;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MapeAda_Middleware.Features.GetBookingsByUser;

public class GetBookingsByUserEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("/api/bookings/user/{nip}", Handle)
            .RequireAuthorization()
            .WithMetadata(new SwaggerOperationAttribute("Obtiene reservas por usuario"))
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status200OK,
                "Reservas del usuario",
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
                "Usuario no encontrado",
                typeof(ProblemDetails)))
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status500InternalServerError,
                "Error no controlado",
                typeof(ProblemDetails)))
            .WithTags("Reservas");
    }

    private static async Task<IResult> Handle(
        [FromRoute][SwaggerParameter("NIP del usuario", Required = true)] string nip,
        IHttpClientFactory httpClientFactory)
    {
        HttpClient client = httpClientFactory.CreateClient(Constants.BackendHttpClientName);

        HttpResponseMessage response = await client.GetAsync($"api/bookings/user/{nip}");

        if (!response.IsSuccessStatusCode)
        {
            return await response.ToProblem();
        }

        IEnumerable<Reserva> bookings = await response.Content.ReadFromJsonAsync<IEnumerable<Reserva>>() ?? throw new InvalidOperationException("Ha ocurrido un error al deserializar el cuerpo de la petición");

        return Results.Ok(bookings);
    }
}
