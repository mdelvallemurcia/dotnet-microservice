using Asp.Versioning.Builder;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

namespace Api.Features.Login.v1;

public class Handler : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        app
            .MapPost("/v{version:apiVersion}/login", Handle)
            .WithSummary("Genera un token de autenticación")
            .WithDescription("Este endpoint genera un token de autenticación para el usuario especificado. El token se puede usar para acceder a otros endpoints protegidos de la API.")
            .WithTags("Auth")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1, 0)
            .AddFluentValidationAutoValidation()
            ;
    }

    private static async Task<IResult> Handle(Request request, IValidator<Request> validator)
    {
        return Results.Ok(new Response { Token = Guid.CreateVersion7().ToString()});
    }
}
