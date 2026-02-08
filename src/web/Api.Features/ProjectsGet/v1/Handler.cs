using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Api.Features.ProjectsGet.v1;

public class Handler : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        app
            .MapGet("/v{version:apiVersion}/projects", Handle)
            .WithSummary("Get a projects list")
            .WithTags("Auth")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1, 0)
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Reader" });
    }

    private static async Task<IResult> Handle()
    {
        var projects = new List<Response>
        {
            new Response { Id = Guid.CreateVersion7(), Name = "Nemo" },
            new Response { Id = Guid.CreateVersion7(), Name = "Poseidon" },
            new Response { Id = Guid.CreateVersion7(), Name = "Luzu" },
        };

        return Results.Ok(projects);
    }
}
