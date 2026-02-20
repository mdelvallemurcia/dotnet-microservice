using Asp.Versioning.Builder;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
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

    internal static async Task<IResult> Handle(Request request, IPublishEndpoint publishEndpoint)
    {
        //TODO store in MongoDb?

        await publishEndpoint.Publish(request.ToEventInsert());

        return Results.Accepted();
    } 
}
