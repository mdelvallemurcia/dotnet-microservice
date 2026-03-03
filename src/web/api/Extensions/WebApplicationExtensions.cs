using Asp.Versioning;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;

namespace Api.Extensions;

public static class WebApplicationExtensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "live";
    private const string ReadynessEndpointPath = "ready";

    public static WebApplication ConfigureOpenApi(this WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options =>
            {
                options
                    .WithTitle("Mi API de Features - .Net 10 style")
                    .WithTheme(ScalarTheme.Default);
            });
        }
        return app;
    }

    public static WebApplication ConfigureApiVersions(this WebApplication app)
    {
        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .HasApiVersion(new ApiVersion(2, 0))
            .ReportApiVersions()
            .Build();

        app.MapDiscoveredEndpoints(versionSet);
        
        return app;
    }

    public static WebApplication MapDefaultHealthChecks(this WebApplication app)
    {
        var isDevelopment = app.Environment.IsDevelopment();

        app.MapHealthChecks(
            $"{HealthEndpointPath}/{AlivenessEndpointPath}", 
            new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains(AlivenessEndpointPath)
            }
        );

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
