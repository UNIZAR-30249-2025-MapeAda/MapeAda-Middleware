using System.Reflection;
using MapeAda_Middleware.Abstract;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MapeAda_Middleware.Extensions;

public static class MapEndpointExtensions
{
    public static IServiceCollection RegisterEndpointsFromAssemblyContaining<T>(this IServiceCollection services)
    {
        Assembly assembly = typeof(T).Assembly;
        
        IEnumerable<Type> endpointTypes = assembly.GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IEndpoint)) && t is { IsClass: true, IsAbstract: false, IsInterface: false });
        
        ServiceDescriptor[] serviceDescriptors = endpointTypes
            .Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type))
            .ToArray();

        services.TryAddEnumerable(serviceDescriptors);
        return services;
    }
    
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        IEnumerable<IEndpoint> endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();
        
        foreach (IEndpoint endpoint in endpoints)
        {
            endpoint.MapEndpoint(app);
        }

        return app;
    }
}