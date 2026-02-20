using Models.Events;

namespace Api.Features.ProjectAdd.v1;

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
}
