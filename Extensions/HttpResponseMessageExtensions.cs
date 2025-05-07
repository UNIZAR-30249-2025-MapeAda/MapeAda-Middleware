using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace MapeAda_Middleware.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static async Task<IResult> ToProblem(this HttpResponseMessage response)
        {
            string raw = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(raw))
            {
                return Results.Problem(statusCode: (int)response.StatusCode);
            }

            try
            {
                ValidationProblemDetails? vpd = JsonSerializer.Deserialize<ValidationProblemDetails>(raw, JsonOptions);
                if (vpd?.Errors != null)
                {
                    return Results.ValidationProblem(
                        vpd.Errors,
                        title: vpd.Title,
                        statusCode: vpd.Status ?? StatusCodes.Status400BadRequest,
                        type: vpd.Type,
                        instance: vpd.Instance
                    );
                }
            }
            catch (JsonException)
            {
                // no era un ValidationProblemDetails
            }

            try
            {
                ProblemDetails? pd = JsonSerializer.Deserialize<ProblemDetails>(raw, JsonOptions);
                if (pd != null)
                {
                    return Results.Problem(
                        detail: pd.Detail,
                        statusCode: pd.Status ?? (int)response.StatusCode,
                        title: pd.Title,
                        type: pd.Type,
                        instance: pd.Instance
                    );
                }
            }
            catch (JsonException)
            {
                // no era un ProblemDetails
            }

            // Fallback: lo devolvemos como texto plano (o HTML)
            return Results.Problem(
                detail: raw.Trim(),
                statusCode: (int)response.StatusCode
            );
        }
    }
}
