using MapeAda_Middleware.Abstract;
using MapeAda_Middleware.Extensions;
using MapeAda_Middleware.SharedModels.Bookings;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MapeAda_Middleware.Features.GetAllBookings;

public class GetAllBookingsEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("/api/bookings", Handle)
            .RequireAuthorization(Constants.GerenteOnlyPolicyName)
            .WithMetadata(new SwaggerOperationAttribute("Obtiene todas las reservas"))
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status200OK,
                "Lista de reservas",
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
                StatusCodes.Status500InternalServerError,
                "Error no controlado",
                typeof(ProblemDetails)))
            .WithTags("Reservas");
    }

    private static async Task<IResult> Handle(
        IHttpClientFactory httpClientFactory)
    {
        HttpClient client = httpClientFactory.CreateClient(Constants.BackendHttpClientName);

        HttpResponseMessage response = await client.GetAsync("api/bookings");

        if (!response.IsSuccessStatusCode)
        {
            return await response.ToProblem();
        }

        IEnumerable<Reserva> bookings = await response.Content.ReadFromJsonAsync<IEnumerable<Reserva>>();

        return Results.Ok(bookings!);
    }
}
