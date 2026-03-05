using Api.Features.Project.ProjectsGet.v1;
using Infrastructure.Repository;
using Microsoft.Extensions.Logging;
using Models.Entity;
using Moq;

namespace Api.Features.Test.Unit.ProjectsGet.v1;

public class Handler_Should
{
    private readonly Mock<IRepository<Project>> _mockProjectRepository = new();
    private readonly Mock<ILogger<Handler>> _mockLogger = new();

    public Handler_Should()
    {
    }

    [Fact]
    public async Task ReturnProjectList_On_HappyPath()
    {
        var result = await Handler.Handle(_mockProjectRepository.Object, _mockLogger.Object);

        Assert.NotNull(result);
        Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Ok>(result);
    }
}
