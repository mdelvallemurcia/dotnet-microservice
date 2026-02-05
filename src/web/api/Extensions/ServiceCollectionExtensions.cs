using Asp.Versioning;

namespace Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureApiVersioning(this IServiceCollection services)
        {
        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                //options.ReportApiVersions = true; // Añade la versión en los headers de respuesta
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            });
        services
            .AddEndpointsApiExplorer();

        return services;
    }
}
