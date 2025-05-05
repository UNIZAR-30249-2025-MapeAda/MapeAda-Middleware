using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MapeAda_Middleware.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task<IResult> ToProblem(this HttpResponseMessage response)
        {
            // Intentamos leer un ValidationProblemDetails
            var vpd = await response.Content
                .ReadFromJsonAsync<ValidationProblemDetails>();

            if (vpd is not null)
            {
                // Devolvemos exactamente los mismos errores y status
                return Results.ValidationProblem(
                    vpd.Errors,
                    title: vpd.Title,
                    statusCode: vpd.Status ?? StatusCodes.Status400BadRequest,
                    type: vpd.Type,
                    instance: vpd.Instance
                );
            }

            // Si no es un ValidationProblemDetails… caemos en Problem normal
            var pd = await response.Content
                .ReadFromJsonAsync<ProblemDetails>();

            return Results.Problem(
                detail: pd?.Detail,
                statusCode: pd?.Status ?? (int)response.StatusCode,
                title: pd?.Title,
                type: pd?.Type,
                instance: pd?.Instance
            );
        }
    }
}
