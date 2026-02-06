using api.Extensions;
using Api.Common;
using Api.Extensions;
using FluentValidation;
using Scalar.AspNetCore;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

var builder = WebApplication.CreateBuilder(args);

//builder.AddServiceDefaults();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services
    .ConfigureApiVersioning()
    .ConfigureBearerTokenGenerator()
    .ConfigureProblemDetails();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Api.Features.IEndpointModule>();

//AUTH builder.Services.AddAuthentication().AddJwtBearer();
//AUTH builder.Services.AddAuthorization(o =>
//{
//    o.AddPolicy("ApiTesterPolicy", b => b.RequireRole("tester"));
//});

//builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler();
app.ConfigureOpenApi();
app.ConfigureApiVersions();

app.UseHttpsRedirection();
//AUTH app.UseAuthorization();

app.Run();

// TODO - capturar excepciones
// TODO - testing
// TODO - worker
