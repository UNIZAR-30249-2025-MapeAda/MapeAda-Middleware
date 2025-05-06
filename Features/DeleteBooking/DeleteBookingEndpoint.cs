using ErrorOr;
using MapeAda_Middleware.Abstract;
using MapeAda_Middleware.Extensions;
using MapeAda_Middleware.SharedModels.Bookings;
using Microsoft.AspNetCore.Authorization;
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
        IHttpClientFactory httpClientFactory,
        HttpContext httpContext,
        IAuthorizationService authorizationService)
    {
        HttpClient client = httpClientFactory.CreateClient(Constants.BackendHttpClientName);
        
        HttpResponseMessage getBookingresponse = await client.GetAsync($"api/bookings/{id}");
        if (!getBookingresponse.IsSuccessStatusCode)
        {
            return await getBookingresponse.ToProblem();
        }
        
        Reserva reserva = await getBookingresponse.Content.ReadFromJsonAsync<Reserva>() ?? throw new InvalidOperationException("Ha ocurrido un error al deserializar el cuerpo de la petición");

        AuthorizationResult authResult = await authorizationService.AuthorizeAsync(httpContext.User, null, Constants.GerenteOnlyPolicyName);
        string nipClaim = httpContext.User.FindFirst(Constants.JwtNipKey)!.Value;
        if (!authResult.Succeeded && nipClaim != reserva.Usuario)
        {
            return Error.Forbidden("No puedes eliminar la reserva de otro usuario").ToProblem();
        }

        if (reserva.DeletedAt is not null)
        {
            return Error.Conflict("La reserva ya ha sido eliminada").ToProblem();
        }
        
        HttpResponseMessage response = await client.DeleteAsync($"api/bookings/{id}?deletedby={nipClaim}");

        if (!response.IsSuccessStatusCode)
        {
            return await response.ToProblem();
        }

        return Results.NoContent();
    }
}
