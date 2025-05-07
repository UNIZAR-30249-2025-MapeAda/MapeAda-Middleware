using System.Net.Http.Headers;
using System.Text;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MapeAda_Middleware.Abstract;
using MapeAda_Middleware.Extensions;
using MapeAda_Middleware.Features.PatchSpace;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
            .WithMetadata(new SwaggerOperationAttribute("Modifica un usuario existente"))
            .Accepts<JsonPatchDocument<PatchUserRequest>>("application/json-patch+json")
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
        [FromRoute][SwaggerParameter("NIP del usuario a modificar", Required = true)] string nip,
        HttpContext context,
        IHttpClientFactory httpClientFactory,
        IValidator<PatchUserRequest> validator)
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

        JsonPatchDocument<PatchUserRequest>? patchDoc;
        try
        {
            patchDoc = JsonConvert.DeserializeObject<JsonPatchDocument<PatchUserRequest>>(body);
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

        string[] propiedades = patchDoc.Operations
            .Select(op => op.path!)
            .Distinct()
            .Select(p => p.TrimStart('/')
                          .Split('/', StringSplitOptions.RemoveEmptyEntries)[0])
            .Select(p => char.ToUpperInvariant(p[0]) + p.Substring(1))
            .Distinct()
            .ToArray();

        PatchUserRequest patchRequest = new();
        List<JsonPatchError> patchErrors = [];
        patchDoc.ApplyTo(patchRequest, err => patchErrors.Add(err));
        if (patchErrors.Count != 0)
        {
            return Error.Validation("ModelState", string.Join("; ", patchErrors.Select(e => e.ErrorMessage))).ToProblem();
        }

        ValidationResult validation = await validator.ValidateAsync(
            patchRequest,
            opts => opts.IncludeProperties(propiedades)
        );

        if (!validation.IsValid)
        {
            IEnumerable<string> msgs = validation.Errors.Select(e => e.ErrorMessage);
            return Error.Validation(
                string.Join(", ", validation.Errors.Select(e => e.PropertyName)),
                string.Join("; ", msgs)
            ).ToProblem();
        }
        
        string payload = JsonConvert.SerializeObject(patchDoc);
        StringContent content = new(payload, Encoding.UTF8, "application/json-patch+json");

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
            if (op.path is null || op.op is null)
            {
                errors.Add("Operación inválida: 'path' u 'op' es null.");
                continue;
            }

            string pathNormalized = op.path.ToLowerInvariant();
            string opNormalized   = op.op.ToLowerInvariant();

            if (pathNormalized == $"/{nameof(PatchUserRequest.Rol).ToLowerInvariant()}"
                || pathNormalized == $"/{nameof(PatchUserRequest.Departamento).ToLowerInvariant()}")
            {
                if (opNormalized != "replace")
                {
                    errors.Add(
                        $"Operación '{op.op}' no permitida en campo '{op.path}'. " +
                        "Solo 'replace' en 'Rol' y 'Departamento'."
                    );
                }
            }
            else
            {
                errors.Add(
                    $"Operación '{op.op}' no permitida en campo '{op.path}'. " +
                    "Solo 'replace' en 'Rol' y 'Departamento'."
                );
            }
        }

        return errors.Count == 0;
    }
}