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

namespace MapeAda_Middleware.Features.PatchSpace;

public class PatchSpaceEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPatch("/api/spaces/{id}", Handle)
            .AddFluentValidationAutoValidation()
            .RequireAuthorization(Constants.GerenteOnlyPolicyName)
            .Produces<NoContent>(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> Handle(
        [FromRoute] string id,
        [FromBody] JsonPatchDocument<PatchSpaceRequest> patchDoc,
        IHttpClientFactory httpClientFactory,
        [FromServices] IValidator<PatchSpaceRequest> validator)
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

    private static bool ValidatePatchOperations(JsonPatchDocument<PatchSpaceRequest> doc, out List<string> errors)
    {
        errors = [];
        foreach (Operation<PatchSpaceRequest>? op in doc.Operations)
        {
            string? path = op.path?.ToLowerInvariant();
            string? operation = op.op?.ToLowerInvariant();

            if (path is not ($"/{nameof(PatchSpaceRequest.Reservable)}" or $"/{nameof(PatchSpaceRequest.Categoria)}" or $"/{nameof(PatchSpaceRequest.Horario)}" or $"/{nameof(PatchSpaceRequest.Propietarios)}") || operation != "replace")
            {
                errors.Add($"Operación '{op.op}' no permitida en campo '{op.path}'. Solo 'replace' en 'Reservable', 'Categoria', 'Horario' y 'Propietarios'.");
            }
        }
        return errors.Count == 0;
    }
}
