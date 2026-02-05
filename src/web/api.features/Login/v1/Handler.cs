using Asp.Versioning.Builder;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

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
            
            ;
    }

    private static async Task<IResult> Handle(Request request, IValidator<Request> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            // Retorna un 400 con un diccionario de errores formateado
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        return Results.Ok(new Response { Token = Guid.CreateVersion7().ToString()});
    }
}
