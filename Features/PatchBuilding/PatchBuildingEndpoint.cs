using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MapeAda_Middleware.Abstract;
using MapeAda_Middleware.Extensions;
using MapeAda_Middleware.SharedModels.Building;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using Swashbuckle.AspNetCore.Annotations;

namespace MapeAda_Middleware.Features.PatchBuilding;

public class PatchBuildingEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPatch("/api/building", Handle)
            .AddFluentValidationAutoValidation()
            .RequireAuthorization(Constants.GerenteOnlyPolicyName)
            .WithMetadata(new SwaggerOperationAttribute("Modifica la configuración del edificio"))
            .Accepts<JsonPatchDocument<Edificio>>("application/json-patch+json")
            .WithMetadata(new SwaggerResponseAttribute(
                StatusCodes.Status204NoContent,
                "Edificio modificado correctamente"))
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
                StatusCodes.Status500InternalServerError,
                "Error no controlado",
                typeof(ProblemDetails)))
            .WithTags("Edificio");
    }

    private static async Task<IResult> Handle(
        HttpContext context,
        IHttpClientFactory httpClientFactory,
        IValidator<Edificio> validator)
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

        JsonPatchDocument<Edificio>? patchDoc;
        try
        {
            patchDoc = JsonConvert.DeserializeObject<JsonPatchDocument<Edificio>>(body);
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

        string payload = JsonConvert.SerializeObject(patchDoc);
        StringContent content = new(payload, Encoding.UTF8, "application/json-patch+json");

        HttpClient client = httpClientFactory.CreateClient(Constants.BackendHttpClientName);
        HttpResponseMessage response = await client.PatchAsync("api/building", content);

        if (!response.IsSuccessStatusCode)
        {
            return await response.ToProblem();
        }

        return Results.NoContent();
    }

    // /calendarioApertura/horariosApertura or /calendarioApertura/horariosApertura/{index}
    private static readonly Regex HorariosRegex = new(
        @"^/calendarioapertura/horariosapertura(?:/(-|\d+))?$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    // /porcentajeUsoMaximo or /porcentajeUsoMaximo/valor
    private static readonly Regex PorcentajeRegex = new(
        @"^/porcentajeusomaximo(?:/valor)?$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    // /calendarioApertura/intervaloPorDefecto or /calendarioApertura/intervaloPorDefecto/inicio|fin
    private static readonly Regex IntervaloRegex = new(
        @"^/calendarioapertura/intervalopordefecto(?:/(inicio|fin))?$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    // /calendarioApertura/diasPorDefecto
    private static readonly Regex DiasRegex = new(
        @"^/calendarioapertura/diaspordefecto$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static bool ValidatePatchOperations(
        JsonPatchDocument<Edificio> doc,
        out List<string> errors)
    {
        errors = [];

        foreach (Operation<Edificio>? op in doc.Operations)
        {
            if (op.path == null || op.op == null)
            {
                errors.Add("Operación inválida: 'path' u 'op' es null.");
                continue;
            }

            string path = op.path.Trim().ToLowerInvariant();
            string operation = op.op.Trim().ToLowerInvariant();

            // 1) HorariosApertura: replace, add, remove
            if (HorariosRegex.IsMatch(path))
            {
                if (operation != "replace"
                    && operation != "add"
                    && operation != "remove")
                {
                    errors.Add(
                        $"Operación '{op.op}' no permitida en '{op.path}'. " +
                        "Sólo 'replace', 'add' o 'remove' en 'CalendarioApertura.HorariosApertura'."
                    );
                }
            }
            // 2) PorcentajeUsoMaximo*, DiasPorDefecto, IntervaloPorDefecto* y CalendarioApertura: sólo replace
            else if (PorcentajeRegex.IsMatch(path)
                  || DiasRegex.IsMatch(path)
                  || IntervaloRegex.IsMatch(path)
                  || path == "/calendarioapertura")
            {
                if (operation != "replace")
                {
                    errors.Add(
                        $"Operación '{op.op}' no permitida en '{op.path}'. " +
                        "Sólo 'replace' está permitido en esta ruta."
                    );
                }
            }
            // 3) Cualquier otro path → inválido
            else
            {
                errors.Add(
                    $"Operación '{op.op}' no permitida en '{op.path}'.\n" +
                    "Rutas válidas:\n" +
                    "- 'replace' en '/PorcentajeUsoMaximo', '/PorcentajeUsoMaximo/Valor',\n" +
                    "  '/CalendarioApertura',\n" +
                    "  '/CalendarioApertura/IntervaloPorDefecto',\n" +
                    "  '/CalendarioApertura/IntervaloPorDefecto/Inicio',\n" +
                    "  '/CalendarioApertura/IntervaloPorDefecto/Fin',\n" +
                    "  '/CalendarioApertura/DiasPorDefecto'.\n" +
                    "- 'replace', 'add' o 'remove' en\n" +
                    "  '/CalendarioApertura/HorariosApertura' y\n" +
                    "  '/CalendarioApertura/HorariosApertura/{índice}'."
                );
            }
        }

        return errors.Count == 0;
    }
}
