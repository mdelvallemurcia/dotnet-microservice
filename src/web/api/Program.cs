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
    .ConfigureBearerTokenGenerator()
    .ConfigureAuthentication(builder.Configuration.GetSection(BearerTokenOptions.Section).Get<BearerTokenOptions>()!)
    .AddAuthorization()
    .ConfigureProblemDetails()
    .AddExceptionHandler<GlobalExceptionHandler>()
    .AddFluentValidationAutoValidation()
    .AddValidatorsFromAssemblyContaining<Api.Features.IEndpointModule>()
    .AddMongoRepository(builder.Configuration.GetSection(MongoDbOptions.Section).Get<MongoDbOptions>()!)
    .ConfigureMassTransit(builder.Configuration.GetSection(RabbitMqOptions.Section).Get<RabbitMqOptions>()!)
    .ConfigureCors()
    .ConfigureOpenTelemetry()
    .ConfigureHealthChecks();

builder
    .AddServiceDefaults();
    //.AddRabbitMQClient("messaging");

builder.Logging    
    .AddOpenTelemetry(logging =>
    {
        var serviceName = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME")??string.Empty;
        logging.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName));
        logging.IncludeFormattedMessage = true;
        logging.IncludeScopes = true;
        logging.AddOtlpExporter();
    });

var app = builder.Build();

app.UseExceptionHandler();

app.ConfigureOpenApi();
app.ConfigureApiVersions();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapDefaultHealthChecks();

app.Run();