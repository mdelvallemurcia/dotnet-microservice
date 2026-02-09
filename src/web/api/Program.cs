using Api.Extensions;
using Api.Common;
using FluentValidation;
using Scalar.AspNetCore;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using Api.Features.Shared.Auth;

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
    .AddValidatorsFromAssemblyContaining<Api.Features.IEndpointModule>();

builder
    .AddServiceDefaults()
    .AddRabbitMQClient("messaging");

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