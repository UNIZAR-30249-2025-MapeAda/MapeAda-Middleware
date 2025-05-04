using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MapeAda_Middleware.Abstract;
using MapeAda_Middleware.Extensions;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using Swashbuckle.AspNetCore.Annotations;

namespace MapeAda_Middleware.Features.PatchSpace;

public class PatchSpaceEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPatch("/api/spaces/{id}", Handle)
            .AddFluentValidationAutoValidation()
            .RequireAuthorization(Constants.GerenteOnlyPolicyName)
            .WithMetadata(new SwaggerOperationAttribute("Modifica un espacio existente"))
            .Accepts<JsonPatchDocument<PatchSpaceRequest>>("application/json-patch+json")
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status204NoContent,
                "Espacio modificado correctamente"))
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
                "Espacio no encontrado",
                typeof(ProblemDetails)))            
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status500InternalServerError,
                "Error no controlado",
                typeof(ProblemDetails)))
            .WithTags("Espacios");
    }

    private static async Task<IResult> Handle(
        [FromRoute][SwaggerParameter("ID del espacio a modificar", Required = true)] string id,
        [FromBody][SwaggerRequestBody("Documento JSON Patch para aplicar cambios", Required = true)] JsonPatchDocument<PatchSpaceRequest> patchDoc,
        IHttpClientFactory httpClientFactory,
        IValidator<PatchSpaceRequest> validator)
    {
        if (!ValidatePatchOperations(patchDoc, out List<string> opErrors))
        {
            return Error.Validation("JsonPatch", string.Join("; ", opErrors)).ToProblem();
        }

        PatchSpaceRequest patchRequest = new();
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
        HttpResponseMessage response = await client.PatchAsync($"api/spaces/{id}", content);

        if (!response.IsSuccessStatusCode)
        {
            return await response.ToProblem();
        }

        return Results.NoContent();
    }

    private static readonly Regex PropietariosArrayRegex = new(
        @"^/propietarios(?:/(-|\d+))?$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex PropietarioPropRegex = new(
        @"^/propietarios/(?:-|\d+)/(tipo|id)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex HorarioRegex = new(
        @"^/horario(?:/(inicio|fin))?$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static bool ValidatePatchOperations(
        JsonPatchDocument<PatchSpaceRequest> doc,
        out List<string> errors)
    {
        errors = new List<string>();

        foreach (Operation<PatchSpaceRequest>? op in doc.Operations)
        {
            if (op.path == null || op.op == null)
            {
                errors.Add("Operación inválida: 'path' u 'op' es null.");
                continue;
            }

            string pathNormalized = op.path.Trim().ToLowerInvariant();
            string opNormalized   = op.op.Trim().ToLowerInvariant();

            // 1) Colección Propietarios (nivel 0 o nivel 1): allow replace, add, remove
            if (PropietariosArrayRegex.IsMatch(pathNormalized))
            {
                if (opNormalized != "replace"
                 && opNormalized != "add"
                 && opNormalized != "remove")
                {
                    errors.Add(
                        $"Operación '{op.op}' no permitida en '{op.path}'. " +
                        "Sólo 'replace', 'add' o 'remove' en 'Propietarios' o sus índices."
                    );
                }
            }
            // 2) Propiedades intrínsecas de un Propietario: sólo replace
            else if (PropietarioPropRegex.IsMatch(pathNormalized))
            {
                if (opNormalized != "replace")
                {
                    errors.Add(
                        $"Operación '{op.op}' no permitida en '{op.path}'. " +
                        "Sólo 'replace' en las propiedades de un 'Propietario'."
                    );
                }
            }
            // 3) Reservable, Categoria o Horario (y sus subpropiedades Inicio/Fin): sólo replace
            else if (pathNormalized == "/reservable"
                  || pathNormalized == "/categoria"
                  || HorarioRegex.IsMatch(pathNormalized))
            {
                if (opNormalized != "replace")
                {
                    errors.Add(
                        $"Operación '{op.op}' no permitida en '{op.path}'. " +
                        "Sólo 'replace' en 'Reservable', 'Categoria' o 'Horario'."
                    );
                }
            }
            // 4) Cualquier otro path: no permitido
            else
            {
                errors.Add(
                    $"Operación '{op.op}' no permitida en '{op.path}'.\n" +
                    "Rutas válidas:\n" +
                    "- 'replace', 'add' o 'remove' en '/Propietarios' o '/Propietarios/{índice}'.\n" +
                    "- 'replace' en '/Propietarios/{índice}/tipo' o '/Propietarios/{índice}/id'.\n" +
                    "- 'replace' en '/Reservable', '/Categoria', '/Horario', '/Horario/Inicio' o '/Horario/Fin'."
                );
            }
        }

        return errors.Count == 0;
    }
}
