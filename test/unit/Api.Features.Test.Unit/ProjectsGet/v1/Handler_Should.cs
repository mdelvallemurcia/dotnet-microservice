using Api.Features.ProjectsGet.v1;

namespace Api.Features.Test.Unit.ProjectsGet.v1;

public class Handler_Should
{
    Handler _handler; 

    public Handler_Should()
    {        
        _handler = new Handler();
    }

    [Fact]
    public async Task ReturnProjectList_On_HappyPath()
    {
        // Act
        var result = await Handler.Handle();
        // Assert
        Assert.NotNull(result);
        Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Ok<List<Response>>> (result);                
    }
}
