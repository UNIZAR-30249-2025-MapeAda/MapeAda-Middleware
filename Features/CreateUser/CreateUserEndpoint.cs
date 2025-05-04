using MapeAda_Middleware.Abstract;
using MapeAda_Middleware.Extensions;
using MapeAda_Middleware.SharedModels.Users;
using Microsoft.AspNetCore.Mvc;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using Swashbuckle.AspNetCore.Annotations;

namespace MapeAda_Middleware.Features.CreateUser;

public class CreateUserEndpoint: IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("/api/users", Handle)
            .AddFluentValidationAutoValidation()
            .RequireAuthorization(Constants.GerenteOnlyPolicyName)
            .WithMetadata(new SwaggerOperationAttribute("Crea un nuevo usuario"))
            .Accepts<CreateUserRequest>("application/json")
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status201Created,
                "Usuario creado",
                typeof(Usuario)))
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status400BadRequest,
                "Datos de reserva inválidos",
                typeof(ProblemDetails)))
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status401Unauthorized,
                "Necesitas iniciar sesión",
                typeof(ProblemDetails)))
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status403Forbidden,
                "No tienes permisos suficientes",
                typeof(ProblemDetails)))
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status409Conflict,
                "El usuario con nip y/o email ya existe",
                typeof(ProblemDetails)))
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status500InternalServerError,
                "Error no controlado",
                typeof(ProblemDetails)))
            .WithTags("Usuarios");
    }

    private static async Task<IResult> Handle(
        [FromBody][SwaggerRequestBody("Datos para crear el usuario", Required = true)] CreateUserRequest request,
        IHttpClientFactory httpClientFactory)
    {
        HttpClient client = httpClientFactory.CreateClient(Constants.BackendHttpClientName);
        
        HttpResponseMessage response = await client.PostAsJsonAsync("api/users", request);

        if (!response.IsSuccessStatusCode)
        {
            return await response.ToProblem();
        }
        
        Usuario usuario = await response.Content.ReadFromJsonAsync<Usuario>() ?? throw new InvalidOperationException("Ha ocurrido un error al deserializar el cuerpo de la petición");
        
        return Results.Created($"/api/users/{request.Nip}", usuario);
    }
}