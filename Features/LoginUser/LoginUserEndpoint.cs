using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using ErrorOr;
using MapeAda_Middleware.Abstract;
using MapeAda_Middleware.Configuration;
using MapeAda_Middleware.Extensions;
using MapeAda_Middleware.SharedModels.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using Swashbuckle.AspNetCore.Annotations;

namespace MapeAda_Middleware.Features.LoginUser;

public class LoginUserEndpoint: IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("/api/auth/login", Handle)
            .AddFluentValidationAutoValidation()
            .WithMetadata(new SwaggerOperationAttribute("Inicia sesión con el email de usuario"))
            .Accepts<LoginUserRequest>("application/json")
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status200OK,
                "Iniciar sesión exitosamente"),
                typeof(LoginUserResponse))
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status400BadRequest,
                "El formato del email es inválido",
                typeof(ProblemDetails)))
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status401Unauthorized,
                "Datos incorrectos",
                typeof(ProblemDetails)))
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status500InternalServerError,
                "Error no controlado",
                typeof(ProblemDetails)))
            .WithTags("Auth");
        
    }

    private static async Task<IResult> Handle(
        [FromBody][SwaggerRequestBody("Datos para iniciar sesión", Required = true)] LoginUserRequest request,
        IHttpClientFactory httpClientFactory,
        IOptions<AuthConfiguration> authOptions)
    {
        HttpClient client = httpClientFactory.CreateClient(Constants.BackendHttpClientName);
        
        HttpResponseMessage response = await client.GetAsync($"api/users?email={request.Email}");

        if (!response.IsSuccessStatusCode)
        {
            return response.StatusCode < HttpStatusCode.InternalServerError
                ? Error.Unauthorized("Credenciales inválidas").ToProblem()
                : await response.ToProblem();
        }
        
        Usuario user = await response.Content.ReadFromJsonAsync<Usuario>() ?? throw new InvalidOperationException("Ha ocurrido un error al deserializar el cuerpo de la petición");

        string token = GenerateJwtToken(user, authOptions.Value);

        return Results.Ok(new LoginUserResponse(token));
    }
    
    private static string GenerateJwtToken(Usuario user, AuthConfiguration authConfiguration)
    {
        SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(authConfiguration.Key));
        SigningCredentials credentials = new(securityKey, SecurityAlgorithms.HmacSha256);

        List<Claim> claims = [
            new(Constants.JwtEmailKey, user.Email),
            new(Constants.JwtNipKey, user.Nip),
            new(Constants.JwtRolKey, user.Rol.ToString())
        ];

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: authConfiguration.Issuer,
            audience: authConfiguration.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}