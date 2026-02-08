using Asp.Versioning;
using Scalar.AspNetCore;

namespace Api.Extensions;

public static class WebApplicationExtensions
{

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

}
