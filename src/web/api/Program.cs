using Api.Extensions;
using Asp.Versioning;
using FluentValidation;
using Scalar.AspNetCore;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

var builder = WebApplication.CreateBuilder(args);

//builder.AddServiceDefaults();

// Add services to the container.

//builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services
    .ConfigureApiVersioning()
    .ConfigureBearerTokenGenerator();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Api.Features.IEndpointModule>();

//AUTH builder.Services.AddAuthentication().AddJwtBearer();
//AUTH builder.Services.AddAuthorization(o =>
//{
//    o.AddPolicy("ApiTesterPolicy", b => b.RequireRole("tester"));
//});

var app = builder.Build();

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

app.UseHttpsRedirection();
//AUTH app.UseAuthorization();
//app.MapControllers();

var versionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1, 0))
    .HasApiVersion(new ApiVersion(2, 0))
    .ReportApiVersions()
    .Build();
app.MapDiscoveredEndpoints(versionSet);

app.Run();


// TODO - capturar excepciones
// TODO - testing
// TODO - worker
