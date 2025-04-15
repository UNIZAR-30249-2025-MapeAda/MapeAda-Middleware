using MapeAda_Middleware.Abstract;
using MapeAda_Middleware.Configuration;
using MapeAda_Middleware.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

namespace MapeAda_Middleware.Features.CreateUser;

public sealed class CreateUserEndpoint: IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("/api/users", Handle)
            .AddFluentValidationAutoValidation()
            .RequireAuthorization(Constants.GerenteOnlyPolicyName);
    }

    private static async Task<IResult> Handle(
        [FromBody] CreateUserRequest request,
        IHttpClientFactory httpClientFactory,
        IOptions<AuthConfiguration> authOptions)
    {
        HttpClient client = httpClientFactory.CreateClient(Constants.BackendHttpClientName);
        
        HttpResponseMessage response = await client.PostAsJsonAsync("api/users", request);

        if (!response.IsSuccessStatusCode)
        {
            return await response.ToProblem();
        }
        
        return Results.Created($"/api/users/{request.Nip}", request);
    }
}