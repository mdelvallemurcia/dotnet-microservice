using Api.Features.Services.Auth;
using Asp.Versioning.Builder;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Models.Entity;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

namespace Api.Features.Auth.Login.v1;

public class Handler : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        app
            .MapPost("/v{version:apiVersion}/login", Handle)
            .WithSummary("Generate a new Auth token")
            .WithTags("Auth")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1, 0)
            .AddFluentValidationAutoValidation()
            .ProducesValidationProblem()
            .AllowAnonymous();
    }

    private static async Task<IResult> Handle(
        HttpContext httpContext,
        Request request,
        IAuthFacade authGenerator,
        IRepository<RefreshToken> refreshTokenRepository
    )
    {
        if (!request.UserName.Equals("miguelito") || !request.Password.Equals("sdlkfj298LJ!"))
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    { "UserName", [ "Invalid data" ] },
                    { "Password", [ "Invalid data" ] }
                });

        var fingerprint = authGenerator.GenerateFingerprint();
        var fingerprintHash = authGenerator.GenerateHash(fingerprint);
        var accessToken = authGenerator.GenerateAccessToken(request.UserName, fingerprintHash);
        var refreshToken = authGenerator.GenerateRefreshToken(request.UserName);

        var refreshTokenEntity = request.ToRefreshToken(refreshToken, fingerprintHash, httpContext, authGenerator);
        await refreshTokenRepository.InsertAsync(refreshTokenEntity);

        // Raw fingerprint in the cookie; only its hash lives in the token and the DB.
        authGenerator.AddSecureCookie(httpContext, CookieKeyEnum.Fingerprint, fingerprint);
        authGenerator.AddSecureCookie(httpContext, CookieKeyEnum.RefreshToken, refreshToken);

        return Results.Ok(new Response { AccessToken = accessToken });
    }

}
