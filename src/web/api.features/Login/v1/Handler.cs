using Api.Features.Shared.Auth;
using Asp.Versioning.Builder;
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
            .WithSummary("Generate a new Auth token")
            .WithTags("Auth")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1, 0)
            .AddFluentValidationAutoValidation();
    }

    private static async Task<IResult> Handle(Request request, IBearerTokenGenerator bearerTokenGenerator)
    {
        return Results.Ok(new Response { Token = bearerTokenGenerator.CreateToken(request.UserName)});
    }
}
