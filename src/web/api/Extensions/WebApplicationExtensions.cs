using System.Security.Cryptography;
using Asp.Versioning;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;

namespace Api.Extensions;

public static class WebApplicationExtensions
{
    private const string _healthEndpointPath = "/health";
    private const string _alivenessEndpointPath = "live";
    private const string _readynessEndpointPath = "ready";

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

    public static WebApplication MapEndpointsAndConfigureApiVersions(this WebApplication app)
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
            $"{_healthEndpointPath}/{_alivenessEndpointPath}",
            new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains(_alivenessEndpointPath)
            }
        );

        app.MapHealthChecks(
            $"{_healthEndpointPath}/{_readynessEndpointPath}",
            new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains(_readynessEndpointPath),
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

    public static WebApplication ConfigureContentSecurityPolicy(this WebApplication app)
    {
        var isDevelopment = app.Environment.IsDevelopment();
        var apiDomain = "localhost:0000";

        // https://csp-evaluator.withgoogle.com/
        var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));


        app.Use(async (context, next) =>
        {
            var headers = context.Response.Headers;
            headers.Append("X-Frame-Options", "DENY");
            headers.Append("X-Content-Type-Options", "nosniff");
            headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

            context.Items["CSPNonce"] = nonce;

            headers.Append(
                "Content-Security-Policy",
                    $"default-src 'self'; script-src 'self' 'nonce-{nonce}';" +
                    (isDevelopment ? $"script-src 'self' 'unsafe-eval' 'unsafe-inline' https://{apiDomain};" : "script-src 'self'; ") +
                    "style-src 'self' 'unsafe-inline'; " +
                    "img-src 'self' data:; " +
                    "font-src 'self'; " +
                    (isDevelopment ? $"connect-src 'self' https://{apiDomain} ws://{apiDomain};" : $"connect-src 'self' https://{apiDomain}; ") +
                    "frame-ancestors 'none'; " +
                    "object-src 'none'; " +
                    "base-uri 'self'; " +
                    "form-action 'self';"
            );

            await next();
        });

        return app;
    }
}
