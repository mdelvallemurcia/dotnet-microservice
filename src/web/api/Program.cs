using api.Middleware;
using api.Options;
using Api.Common;
using Api.Extensions;
using Api.Features.Shared.Auth;
using Aspire.ServiceDefaults;
using FluentValidation;
using Infrastructure.Extensions;
using Infrastructure.Repository;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services
    .ConfigureApiVersioning()
    .AddInternalServices()
    .ConfigureAuthentication(builder.Configuration.GetSection(BearerTokenOptions.Section).Get<BearerTokenOptions>()!)
    .ConfigureAuthorization()
    .ConfigureProblemDetails()
    .AddExceptionHandler<GlobalExceptionHandler>()
    .AddFluentValidationAutoValidation()
    .AddValidatorsFromAssemblyContaining<Api.Features.IEndpointModule>()
    .AddMongoRepository(builder.Configuration.GetSection(MongoDbOptions.Section).Get<MongoDbOptions>()!)
    .ConfigureMassTransit(builder.Configuration.GetSection(RabbitMqOptions.Section).Get<RabbitMqOptions>()!)
    .ConfigureCors(builder.Configuration.GetSection(CorsOptions.Section).Get<CorsOptions>()?.AllowedOrigins ?? [])
    .ConfigureForwardedHeaders()
    .ConfigureOpenTelemetry()
    .ConfigureHealthChecks();

builder
    .AddServiceDefaults();
//.AddRabbitMQClient("messaging");

builder.Logging
    .AddOpenTelemetry(logging =>
    {
        var serviceName = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME") ?? string.Empty;
        logging.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName));
        logging.IncludeFormattedMessage = true;
        logging.IncludeScopes = true;
        logging.AddOtlpExporter();
    });

var app = builder.Build();

// Must run before any middleware that reads the scheme/remote IP (HTTPS redirect, auth, cookies).
app.UseForwardedHeaders();

app.UseExceptionHandler();
app.UseCors("DefaultPolicy");
//app.ConfigureContentSecurityPolicy(); //TODO check!! ---------------------

app.ConfigureOpenApi();

// In Development the SPA reaches the API through Vite's HTTP proxy; redirecting to HTTPS would
// break that hop (and force Secure cookies an http page can't store). Keep it for prod only.
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<FingerprintValidationMiddleware>();
app.MapDefaultHealthChecks();
app.MapEndpointsAndConfigureApiVersions();

app.Run();
