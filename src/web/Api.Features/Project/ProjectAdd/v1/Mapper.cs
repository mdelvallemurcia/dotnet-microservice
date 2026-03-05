using Models.Entity;
using Models.Events.Project;

namespace Api.Features.Project.ProjectAdd.v1;

internal static class Mapper
{
    internal static ProjectAdded ToEventInsert(this Request request)
    {
        return new ProjectAdded
        (
            request.Id,
            request.CreationDate,
            request.Name
        );
    }

    internal static Project ToProjectEntity(this Request request)
    {
        return new Project
        (
            request.CreationDate,
            request.Name
        )
        { Id = request.Id };
    }
}
