using MapeAda_Middleware.Abstract;
using MapeAda_Middleware.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace MapeAda_Middleware.Features.DeleteBooking;

public class DeleteBookingEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapDelete("/api/bookings/{id}", Handle)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblems(StatusCodes.Status401Unauthorized, StatusCodes.Status403Forbidden, StatusCodes.Status404NotFound, StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> Handle(
        [FromRoute] int id,
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
