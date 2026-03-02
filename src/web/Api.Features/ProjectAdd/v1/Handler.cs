using Asp.Versioning.Builder;
using Infrastructure.Repository;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Models.Entity;
using Models.Events.Project;
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

    internal static async Task<IResult> Handle(
        Request request, 
        IPublishEndpoint publishEndpoint, 
        IRepository<Project> projectRepository, 
        IRepository<EventFailure<ProjectAdded>> eventFailureRepository, 
        ILogger<Handler> logger,
        CancellationToken cancellationToken)
    {
        var project = await projectRepository.GetByIdAsync(request.Id, cancellationToken);
        if (project != null)
            return Results.Conflict($"Project with id {request.Id} already exists.");

        project = request.ToProjectEntity();
        await projectRepository.InsertAsync(project, cancellationToken);

        var @event = request.ToEventInsert();
        try
        {
            var ct = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await publishEndpoint.Publish(@event, ct.Token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to publish ProjectAdded event for project {ProjectId}", request.Id);
            await eventFailureRepository.InsertAsync(new(@event));
            return Results.InternalServerError(); //TODO adaptar a la excepción
        }

        logger.LogInformation("Project created: {ProjectId}", project.Id);
        return Results.Accepted();
    } 
}
