using MapeAda_Middleware.Abstract;
using MapeAda_Middleware.Extensions;
using MapeAda_Middleware.SharedModels.Buiding;

namespace MapeAda_Middleware.Features.GetBuilding;

public class GetBuildingEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("/api/building", Handle)
            .RequireAuthorization()
            .Produces<IEnumerable<Edificio>>(StatusCodes.Status200OK)
            .ProducesProblems(StatusCodes.Status401Unauthorized, StatusCodes.Status403Forbidden, StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> Handle(
        IHttpClientFactory httpClientFactory)
    {
        HttpClient client = httpClientFactory.CreateClient(Constants.BackendHttpClientName);

        HttpResponseMessage response = await client.GetAsync("api/building");

        if (!response.IsSuccessStatusCode)
        {
            return await response.ToProblem();
        }

        Edificio edificio = await response.Content.ReadFromJsonAsync<Edificio>();

        return Results.Ok(edificio!);
    }
}
