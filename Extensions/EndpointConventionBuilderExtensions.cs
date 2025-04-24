namespace MapeAda_Middleware.Extensions;


public static class EndpointConventionBuilderExtensions
{
    public static TEndpoint ProducesProblems<TEndpoint>(
        this TEndpoint builder,
        params int[] statusCodes)
        where TEndpoint : IEndpointConventionBuilder
    {
        foreach (int code in statusCodes)
        {
            builder.ProducesProblem(code);
        }
        return builder;
    }
}