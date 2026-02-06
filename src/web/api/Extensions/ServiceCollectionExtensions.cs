using Api.Features.Shared.Auth;
using Asp.Versioning;
using System.Diagnostics;

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

    public static IServiceCollection ConfigureBearerTokenGenerator  (this IServiceCollection services)
    {
        services
            .AddOptions<BearerTokenOptions>()
            .BindConfiguration(BearerTokenOptions.Section);

        services.AddSingleton<IBearerTokenGenerator, BearerTokenGenerator>();            

        return services;
    }


    public static IServiceCollection ConfigureProblemDetails(this IServiceCollection services)
    {
        services
            .AddProblemDetails(po => po.CustomizeProblemDetails = pc =>
            {
                pc.ProblemDetails.Extensions.Add("id", Activity.Current.Id);
                pc.ProblemDetails.Extensions.Add("spanId", Activity.Current.SpanId.ToString());
                pc.ProblemDetails.Extensions.Add("traceId", Activity.Current.TraceId.ToString());                
            });

        return services;
    }
}