using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace ProjectSubscriber.Extensions;

public static class WebApplicationExtensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "live";
    private const string ReadynessEndpointPath = "ready";

    public static WebApplication MapDefaultHealthChecks(this WebApplication app)
    {        
        app.MapHealthChecks(
            $"{HealthEndpointPath}/{AlivenessEndpointPath}",
            new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains(AlivenessEndpointPath)
            }
        );

        var isDevelopment = app.Environment.IsDevelopment();
        app.MapHealthChecks(
            $"{HealthEndpointPath}/{ReadynessEndpointPath}",
            new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains(ReadynessEndpointPath),
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var result = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        status = report.Status.ToString(),
                        details = report.Entries.Select(e => new
                        {
                            key = e.Key,
                            status = e.Value.Status.ToString(),
                            description = isDevelopment ? e.Value.Description : default,
                            exception = isDevelopment ? e.Value.Exception?.Message : default
                        })
                    });
                    await context.Response.WriteAsync(result);
                }
            }
        );

        return app;
    }
}

