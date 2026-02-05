using Api.Features;
using Asp.Versioning.Builder;

namespace api.Extensions;

public static class EndpointDiscoveryExtensions
{
    public static void MapDiscoveredEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var featureAssembly = typeof(IEndpointModule).Assembly;

        var modules = featureAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IEndpointModule).IsAssignableFrom(t));

        foreach (var moduleType in modules)
        {
            var module = (IEndpointModule)Activator.CreateInstance(moduleType)!;

            // Pasamos el versionSet a cada módulo descubierto
            module.MapEndpoints(app, versionSet);
        }
    }
}