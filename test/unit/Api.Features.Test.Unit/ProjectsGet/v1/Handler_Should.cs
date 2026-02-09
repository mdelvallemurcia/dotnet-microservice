using Api.Features.ProjectsGet.v1;

namespace Api.Features.Test.Unit.ProjectsGet.v1;

public class Handler_Should
{
    public Handler_Should()
    {        
    }

    [Fact]
    public async Task ReturnProjectList_On_HappyPath()
    {
        var result = await Handler.Handle();

        Assert.NotNull(result);
        Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Ok<List<Response>>> (result);                
    }
}
