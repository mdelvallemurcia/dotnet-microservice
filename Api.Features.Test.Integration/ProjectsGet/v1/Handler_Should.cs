using Api.Features.Shared.Auth;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;

namespace Api.Features.Test.Integration.ProjectsGet.v1;

public class Handler_Should : ApiTestBase
{
    private readonly IBearerTokenGenerator _bearerTokenGenerator;

    public Handler_Should(WebApplicationFactory<Program> factory) : base(factory)
    {
        var scope = factory.Services.CreateScope();
        _bearerTokenGenerator = scope.ServiceProvider.GetRequiredService<IBearerTokenGenerator>();
    }

    [Fact]
    public async Task ReturnUnauthorized_When_TokenIsInvalid()
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");

        var response = await Client.GetAsync("/v1/projects");

        Assert.NotNull(response);
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ReturnOk_When_TokenIsValid()
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _bearerTokenGenerator.CreateToken("User"));

        var response = await Client.GetAsync("/v1/projects");

        Assert.NotNull(response);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }
}
