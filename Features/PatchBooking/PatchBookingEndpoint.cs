using System.Net.Http.Headers;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MapeAda_Middleware.Abstract;
using MapeAda_Middleware.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using Swashbuckle.AspNetCore.Annotations;

namespace MapeAda_Middleware.Features.PatchBooking;
public class PatchBookingEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPatch("/api/bookings/{id}", Handle)
            .AddFluentValidationAutoValidation()
            .RequireAuthorization(Constants.GerenteOnlyPolicyName)
            .WithMetadata(new SwaggerOperationAttribute("Modifica una reserva existente"))
            .Accepts<JsonPatchDocument<PatchBookingRequest>>("application/json-patch+json")
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status204NoContent,
                "Reserva modificada exitosamente"))
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
                "Reserva no encontrada",
                typeof(ProblemDetails)))
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status500InternalServerError,
                "Error no controlado",
                typeof(ProblemDetails)))
            .WithTags("Reservas");;
    }

    private static async Task<IResult> Handle(
        [FromRoute][SwaggerParameter("ID de la reserva a modificar", Required = true)] int id,
        [FromBody][SwaggerRequestBody("Documentos JSON Patch para aplicar cambios", Required = true)] JsonPatchDocument<PatchBookingRequest> patchDoc,
        IHttpClientFactory httpClientFactory,
        [FromServices] IValidator<PatchBookingRequest> validator)
    {
        if (!ValidatePatchOperations(patchDoc, out List<string> opErrors))
        {
            return Error.Validation("JsonPatch", string.Join("; ", opErrors)).ToProblem();
        }

        PatchBookingRequest patchRequest = new();
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
        HttpResponseMessage response = await client.PatchAsync($"api/bookings/{id}", content);

        if (!response.IsSuccessStatusCode)
        {
            return await response.ToProblem();
        }

        return Results.NoContent();
    }

    private static bool ValidatePatchOperations(JsonPatchDocument<PatchBookingRequest> doc, out List<string> errors)
    {
        errors = [];
        foreach (Operation<PatchBookingRequest>? op in doc.Operations)
        {
            string? path = op.path?.ToLowerInvariant();
            string? operation = op.op?.ToLowerInvariant();

            if (path is not ($"/{nameof(PatchBookingRequest.Valida)}") || operation != "replace")
            {
                errors.Add($"Operación '{op.op}' no permitida en campo '{op.path}'. Solo 'replace' en 'Valida'.");
            }
        }
        return errors.Count == 0;
    }
}
