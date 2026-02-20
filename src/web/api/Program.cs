using api.Options;
using Api.Common;
using Api.Extensions;
using Api.Features.Shared;
using Api.Features.Shared.Auth;
using Aspire.ServiceDefaults;
using FluentValidation;
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
    .AddFluentValidationAutoValidation()
    .AddExceptionHandler<GlobalExceptionHandler>()
    .AddValidatorsFromAssemblyContaining<Api.Features.IEndpointModule>()
    .ConfigureMassTransit(builder.Configuration.GetSection(RabbitMqOptions.Section).Get<RabbitMqOptions>()!)
    .AddInternalServices();

builder
    .AddServiceDefaults()
    .AddRabbitMQClient("messaging");

builder.Logging.AddConsole();

var app = builder.Build();

app.UseExceptionHandler();
app.MapDefaultEndpoints();
app.ConfigureOpenApi();
app.ConfigureApiVersions();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.Run();

// TODO - testing e2e
// TODO - worker
// TODO healthchecks
// Aspire?
// RabbitMq?
// mass transit?