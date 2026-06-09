using Api.Features.Services.Auth;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Api.Features.Auth.Logout.v1;

public class Handler : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        app
            .MapPost("/v{version:apiVersion}/logout", Handle)
            .WithSummary("Remove the fingerprint and the refresh token from client")
            .WithTags("Auth")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1, 0);
    }

    private static async Task<IResult> Handle(HttpContext httpContext, IAuthFacade authGenerator)
    {
        authGenerator.RemoveSecureCookie(httpContext, CookieKeyEnum.Fingerprint);
        authGenerator.RemoveSecureCookie(httpContext, CookieKeyEnum.RefreshToken);

        return Results.Ok();
    }
}
