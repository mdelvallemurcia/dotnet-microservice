namespace Api.Features.Project.ProjectsGet.v1;

public class Response
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
