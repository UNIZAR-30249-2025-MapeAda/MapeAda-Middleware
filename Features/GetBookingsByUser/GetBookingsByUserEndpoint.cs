using MapeAda_Middleware.Abstract;
using MapeAda_Middleware.Extensions;
using MapeAda_Middleware.SharedModels.Bookings;
using Microsoft.AspNetCore.Mvc;

namespace MapeAda_Middleware.Features.GetUserBookings;

public class GetBookingsByUserEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("/api/bookings/user/{nip}", Handle)
            .RequireAuthorization()
            .Produces<IEnumerable<Reserva>>(StatusCodes.Status200OK)
            .ProducesProblems(StatusCodes.Status401Unauthorized, StatusCodes.Status403Forbidden, StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> Handle(
        [FromRoute] string nip,
        IHttpClientFactory httpClientFactory)
    {
        HttpClient client = httpClientFactory.CreateClient(Constants.BackendHttpClientName);

        HttpResponseMessage response = await client.GetAsync($"api/bookings/user/{nip}");

        if (!response.IsSuccessStatusCode)
        {
            return await response.ToProblem();
        }

        IEnumerable<Reserva> bookings = await response.Content.ReadFromJsonAsync<IEnumerable<Reserva>>();

        return Results.Ok(bookings!);
    }
}
