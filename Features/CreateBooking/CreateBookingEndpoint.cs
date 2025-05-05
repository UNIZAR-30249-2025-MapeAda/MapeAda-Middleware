using MapeAda_Middleware.Abstract;
using MapeAda_Middleware.Extensions;
using MapeAda_Middleware.SharedModels.Bookings;
using Microsoft.AspNetCore.Mvc;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using Swashbuckle.AspNetCore.Annotations;

namespace MapeAda_Middleware.Features.CreateBooking;

public class CreateBookingEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("/api/bookings", Handle)
            .AddFluentValidationAutoValidation()
            .RequireAuthorization()
            .WithMetadata(new SwaggerOperationAttribute("Crea una nueva reserva"))
            .Accepts<CreateBookingRequest>("application/json")
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status201Created,
                "Reserva creada",
                typeof(Reserva)))
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status400BadRequest,
                "Datos de reserva inválidos",
                typeof(ProblemDetails)))
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status401Unauthorized,
                "Necesitas iniciar sesión",
                typeof(ProblemDetails)))
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status409Conflict,
                "La reserva solapa con otra",
                typeof(ProblemDetails)))
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status500InternalServerError,
                "Error no controlado",
                typeof(ProblemDetails)))
            .WithTags("Reservas");
    }

    private static async Task<IResult> Handle(
        [FromBody][SwaggerRequestBody("Datos para crear la reserva", Required = true)] CreateBookingRequest request,
        IHttpClientFactory httpClientFactory,
        HttpContext httpContext)
    {
        HttpClient client = httpClientFactory.CreateClient(Constants.BackendHttpClientName);

        string nipClaim = httpContext.User.FindFirst(Constants.JwtNipKey)!.Value;
        HttpResponseMessage response = await client.PostAsJsonAsync("api/bookings", request.ToBackendRequest(nipClaim));

        if (!response.IsSuccessStatusCode)
        {
            return await response.ToProblem();
        }

        Reserva reserva = await response.Content.ReadFromJsonAsync<Reserva>() ?? throw new InvalidOperationException("Ha ocurrido un error al deserializar el cuerpo de la petición");

        return Results.Created($"/api/bookings/{reserva.Id}", reserva);
    }
}
