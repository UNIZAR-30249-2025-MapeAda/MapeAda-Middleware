using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using SystemTextJsonPatch;

namespace MapeAda_Middleware.Swagger;

public class JsonPatchDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // 1) Elimina todos los esquemas generados para OperationOf… y JsonPatchDocumentOf…
        var toRemove = swaggerDoc.Components.Schemas
            .Keys
            .Where(key =>
                key.StartsWith("Operation") ||
                key.StartsWith("JsonPatchDocument") ||
                key.StartsWith("SystemTextJsonPatch")) // para quien use SystemTextJsonPatch
            .ToList();
        foreach (var key in toRemove)
            swaggerDoc.Components.Schemas.Remove(key);

        // 2) Registra un esquema Operation
        swaggerDoc.Components.Schemas.Add("Operation", new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["op"]    = new OpenApiSchema { Type = "string" , Description = "'add','remove','replace','move','copy','test'" },
                ["path"]  = new OpenApiSchema { Type = "string" , Description = "JSON Pointer al campo destino" },
                ["value"] = new OpenApiSchema { Nullable = true  , Description = "Valor (omitido en 'remove')" },
                ["from"]  = new OpenApiSchema { Type = "string", Nullable = true , Description = "Origen para 'move' o 'copy'" }
            },
            Required = new HashSet<string> { "op", "path" }
        });

        // 3) Registra el esquema JsonPatchDocument como array de Operation
        swaggerDoc.Components.Schemas.Add("JsonPatchDocument", new OpenApiSchema
        {
            Type        = "array",
            Items       = new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "Operation" } },
            Description = "Array de operaciones JSON Patch"
        });

        // 4) Reconfigura todos los endpoints PATCH para que usen solo application/json-patch+json
        foreach (var pathItem in swaggerDoc.Paths.Values)
        {
            if (!pathItem.Operations.TryGetValue(OperationType.Patch, out var patchOp) || patchOp.RequestBody == null)
                continue;

            // Mantén sólo application/json-patch+json
            var ctos = patchOp.RequestBody.Content.Keys
                .Where(ct => ct != "application/json-patch+json")
                .ToList();
            foreach (var ct in ctos)
                patchOp.RequestBody.Content.Remove(ct);

            // Asigna el esquema al body
            patchOp.RequestBody.Content["application/json-patch+json"] = new OpenApiMediaType
            {
                Schema = new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "JsonPatchDocument" } }
            };
        }
    }
}