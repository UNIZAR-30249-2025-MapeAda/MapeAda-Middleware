using MapeAda_Middleware.Abstract;
using MapeAda_Middleware.Extensions;
using MapeAda_Middleware.SharedModels.Bookings;
using Microsoft.AspNetCore.Mvc;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

namespace MapeAda_Middleware.Features.CreateBooking;

public class CreateBookingEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("/api/bookings", Handle)
            .AddFluentValidationAutoValidation()
            .Produces<CreateBookingRequest>(StatusCodes.Status201Created)
            .ProducesProblems(StatusCodes.Status400BadRequest, StatusCodes.Status401Unauthorized, StatusCodes.Status403Forbidden, StatusCodes.Status409Conflict, StatusCodes.Status500InternalServerError)
            .RequireAuthorization();
    }

    private static async Task<IResult> Handle(
        [FromBody] CreateBookingRequest request,
        IHttpClientFactory httpClientFactory)
    {
        HttpClient client = httpClientFactory.CreateClient(Constants.BackendHttpClientName);

        // TODO: Añadir nip del usuario

        HttpResponseMessage response = await client.PostAsJsonAsync("api/bookings", request);

        if (!response.IsSuccessStatusCode)
        {
            return await response.ToProblem();
        }

        Reserva reserva = (await response.Content.ReadFromJsonAsync<Reserva>())!;

        return Results.Created($"/api/bookings/{reserva.Id}", request);
    }
}
