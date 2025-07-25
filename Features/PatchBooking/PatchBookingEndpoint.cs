﻿using System.Net.Http.Headers;
using System.Text;
using ErrorOr;
using MapeAda_Middleware.Abstract;
using MapeAda_Middleware.Extensions;
using MapeAda_Middleware.SharedModels.Bookings;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;

namespace MapeAda_Middleware.Features.PatchBooking;
public class PatchBookingEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPatch("/api/bookings/{id}", Handle)
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
            .WithTags("Reservas");
    }

    private static async Task<IResult> Handle(
        [FromRoute][SwaggerParameter("ID de la reserva a modificar", Required = true)] int id,
        HttpContext context,
        IHttpClientFactory httpClientFactory)
    {
        string? contentType = context.Request.ContentType;
        if (string.IsNullOrEmpty(contentType) || !contentType.StartsWith("application/json-patch+json", StringComparison.OrdinalIgnoreCase))
        {
            return Results.Problem(
                detail: "El Content-Type debe ser 'application/json-patch+json'.",
                statusCode: StatusCodes.Status415UnsupportedMediaType);
        }

        string body;
        using (StreamReader sr = new StreamReader(context.Request.Body))
        {
            body = await sr.ReadToEndAsync();
        }

        JsonPatchDocument<PatchBookingRequest>? patchDoc;
        try
        {
            patchDoc = JsonConvert.DeserializeObject<JsonPatchDocument<PatchBookingRequest>>(body);
        }
        catch (JsonException je)
        {
            return Error.Validation("JsonPatch", je.Message).ToProblem();
        }

        if (patchDoc is null)
        {
            return Error.Validation("JsonPatch", "Documento JSON Patch inválido").ToProblem();
        }

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

        HttpClient client = httpClientFactory.CreateClient(Constants.BackendHttpClientName);
        HttpResponseMessage getBookingresponse = await client.GetAsync($"api/bookings/{id}");
        if (!getBookingresponse.IsSuccessStatusCode)
        {
            return await getBookingresponse.ToProblem();
        }
        
        Reserva reserva = await getBookingresponse.Content.ReadFromJsonAsync<Reserva>() ?? throw new InvalidOperationException("Ha ocurrido un error al deserializar el cuerpo de la petición");

        if (reserva.DeletedAt is not null)
        {
            return Error.Conflict("No puedes editar una reserva eliminada").ToProblem();
        }
        
        string payload = JsonConvert.SerializeObject(patchDoc);
        StringContent content = new(payload, Encoding.UTF8, "application/json-patch+json");

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

            if (path is not "/valida" || operation != "replace")
            {
                errors.Add($"Operación '{op.op}' no permitida en campo '{op.path}'. Solo 'replace' en 'Valida'.");
            }
        }
        return errors.Count == 0;
    }
}
