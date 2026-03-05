namespace Api.Features.Projects.ProjectAdd.v1;

public record Request(
    Guid Id,
    DateTime CreationDate,
    string Name
);
