namespace Api.Features.Project.ProjectAdd.v1;

public record Request(
    Guid Id,
    DateTime CreationDate,
    string Name
);
