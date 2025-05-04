using System.Net.Http.Headers;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MapeAda_Middleware.Abstract;
using MapeAda_Middleware.Extensions;
using MapeAda_Middleware.Features.PatchBooking;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using Swashbuckle.AspNetCore.Annotations;

namespace MapeAda_Middleware.Features.PatchUser;

public class PatchUserEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPatch("/api/users/{nip}", Handle)
            .AddFluentValidationAutoValidation()
            .RequireAuthorization(Constants.GerenteOnlyPolicyName)
            .Produces<NoContent>(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithMetadata(new SwaggerOperationAttribute("Modifica un usuario existente"))
            .Accepts<JsonPatchDocument<PatchBookingRequest>>("application/json-patch+json")
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status204NoContent,
                "Usuario modificado correctamente"))
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status400BadRequest,
                "Documentos JSON Patch inválidos o datos de validación erróneos",
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
                StatusCodes.Status404NotFound,
                "Usuario no encontrado",
                typeof(ProblemDetails)))            
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status409Conflict,
                "Entradas del calendario duplicadas",
                typeof(ProblemDetails)))
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status500InternalServerError,
                "Error no controlado",
                typeof(ProblemDetails)))
            .WithTags("Usuarios");
    }
    
    private static async Task<IResult> Handle(
        [FromRoute] string nip,
        [FromBody] JsonPatchDocument<PatchUserRequest> patchDoc,
        IHttpClientFactory httpClientFactory,
        [FromServices] IValidator<PatchUserRequest> validator)
    {
        if (!ValidatePatchOperations(patchDoc, out List<string> opErrors))
        {
            return Error.Validation("JsonPatch", string.Join("; ", opErrors)).ToProblem();
        }
        
        PatchUserRequest patchRequest = new();
        List<JsonPatchError> patchErrors = [];
        patchDoc.ApplyTo(patchRequest, err => patchErrors.Add(err));
        if (patchErrors.Count != 0)
        {
            return Error.Validation("ModelState", string.Join("; ", patchErrors.Select(e => e.ErrorMessage))).ToProblem();
        }
        
        ValidationResult? validation = await validator.ValidateAsync(patchRequest);
        if (!validation.IsValid)
        {
            IEnumerable<string> messages = validation.Errors.Select(e => e.ErrorMessage);
            return Error.Validation(string.Join(", ", validation.Errors.Select(e => e.PropertyName)), string.Join("; ", messages)).ToProblem();
        }

        JsonContent content = JsonContent.Create(
            patchDoc,
            mediaType: new MediaTypeHeaderValue("application/json-patch+json")
        );

        HttpClient client = httpClientFactory.CreateClient(Constants.BackendHttpClientName);
        HttpResponseMessage response = await client.PatchAsync($"api/users/{nip}", content);

        if (!response.IsSuccessStatusCode)
        {
            return await response.ToProblem();
        }
        
        return Results.NoContent();
    }
    
    private static bool ValidatePatchOperations(JsonPatchDocument<PatchUserRequest> doc, out List<string> errors)
    {
        errors = [];
        foreach (Operation<PatchUserRequest>? op in doc.Operations)
        {
            string? path = op.path?.ToLowerInvariant();
            string? operation = op.op?.ToLowerInvariant();

            if (path is not ($"/{nameof(PatchUserRequest.Rol)}" or $"/{nameof(PatchUserRequest.Departamento)}") || operation != "replace")
            {
                errors.Add($"Operación '{op.op}' no permitida en campo '{op.path}'. Solo 'replace' en 'Rol' y 'Departamento'.");
            }
        }
        return errors.Count == 0;
    }
}