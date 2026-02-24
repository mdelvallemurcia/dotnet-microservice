using Asp.Versioning.Builder;
using Infrastructure.Repository;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Models.Entity;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

namespace Api.Features.ProjectAdd.v1;

public class Handler : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        app
            .MapPost("/v{version:apiVersion}/project", Handle)
            .WithSummary("Create a new project")
            .WithTags("Project")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1, 0)
            .AddFluentValidationAutoValidation()
            .RequireAuthorization();
    }

    internal static async Task<IResult> Handle(Request request, IPublishEndpoint publishEndpoint, IRepository<Project> repository, CancellationToken cancellationToken)
    {
        var project = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (project != null)
            return Results.Conflict($"Project with id {request.Id} already exists.");

        project = request.ToProjectEntity();
        await repository.InsertAsync(project, cancellationToken);
        await publishEndpoint.Publish(request.ToEventInsert());

        return Results.Accepted();
    } 
}
