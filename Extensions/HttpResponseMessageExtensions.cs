using Microsoft.AspNetCore.Mvc;

namespace MapeAda_Middleware.Extensions;

public static class HttpResponseMessageExtensions
{
    public static async Task<IResult> ToProblem(this HttpResponseMessage response)
    {
        ValidationProblemDetails? problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        return Results.Problem(
            detail: string.Join("; ", problemDetails?.Errors.SelectMany(e => e.Value) ?? []),
            statusCode: problemDetails?.Status ?? (int)response.StatusCode,
            title: problemDetails?.Title,
            type: problemDetails?.Type);
    }
}