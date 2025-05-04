using MapeAda_Middleware.Abstract;
using MapeAda_Middleware.Extensions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MapeAda_Middleware.Features.DeleteBooking;

public class DeleteBookingEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapDelete("/api/bookings/{id}", Handle)
            .RequireAuthorization()
            .WithMetadata(new SwaggerOperationAttribute("Elimina una reserva existente"))
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status204NoContent,
                "Reserva eliminada exitosamente"))
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
                "Reserva no encontrada",
                typeof(ProblemDetails)))
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status500InternalServerError,
                "Error no controlado",
                typeof(ProblemDetails)))
            .WithTags("Reservas");
    }

    private static async Task<IResult> Handle(
        [FromRoute][SwaggerParameter("ID de la reserva a eliminar", Required = true)] int id,
        IHttpClientFactory httpClientFactory)
    {
        HttpClient client = httpClientFactory.CreateClient(Constants.BackendHttpClientName);

        HttpResponseMessage response = await client.DeleteAsync($"api/bookings/{id}");

        if (!response.IsSuccessStatusCode)
        {
            return await response.ToProblem();
        }

        return Results.NoContent();
    }
}
