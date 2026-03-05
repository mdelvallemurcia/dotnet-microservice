using Asp.Versioning.Builder;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Models.Entity;

namespace Api.Features.ProjectsGet.v1;

public class Handler : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        app
            .MapGet("/v{version:apiVersion}/projects", Handle)
            .WithSummary("Get a projects list")
            .WithTags("Project")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1, 0)
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Reader" });
    }

    internal static async Task<IResult> Handle(
        IRepository<Project> projectRepository,
        ILogger<Handler> logger,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10
    )
    {
        logger.LogInformation("Getting projects");

        var pagedData = await projectRepository.GetPagedAsync(
            page,
            pageSize
        );

        return Results.Ok(pagedData);
    }
}
