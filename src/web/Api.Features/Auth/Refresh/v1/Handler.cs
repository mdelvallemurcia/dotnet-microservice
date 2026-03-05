using Api.Features.Services.Auth;
using Asp.Versioning.Builder;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Models.Entity;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

namespace Api.Features.Auth.Refresh.v1;

public class Handler : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        app
            .MapPost("/v{version:apiVersion}/refresh", Handle)
            .WithSummary("Check the current refresh token and generates a new one")
            .WithTags("Auth")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1, 0)
            .AddFluentValidationAutoValidation()
            .ProducesValidationProblem()
            .AddFluentValidationAutoValidation()
            .AllowAnonymous();
    }

    private static async Task<IResult> Handle(HttpContext httpContext, Request request, IAuthFacade authGenerator, IRepository<RefreshToken> refreshTokenRepository, CancellationToken cancellationToken)
    {
        var tokenHash = authGenerator.GenerateHash(request.Token);
        var refreshTokens = await refreshTokenRepository.FilterAsync(w => w.Hash.Equals(tokenHash, StringComparison.Ordinal), cancellationToken);
        if (refreshTokens is null || !refreshTokens.Any())
            return Results.BadRequest();

        if (refreshTokens.First().IsExpired)
        {
            await HandleReuseAttack(refreshTokenRepository, refreshTokens.First(), cancellationToken);
            return Results.BadRequest();
        }

        var newRefreshToken = authGenerator.GenerateRefreshToken(refreshTokens.First().UserName);
        var newRefreshTokenHash = authGenerator.GenerateHash(newRefreshToken);
        var oldRefreshTokenEntity = refreshTokens.First() with
        {
            RevokedAt = DateTime.UtcNow,
            ReplacedByHash = newRefreshTokenHash,
            RevocationReason = "Already used"
        };
        await refreshTokenRepository.UpdateAsync(oldRefreshTokenEntity, cancellationToken);

        // TODO config expiration -------------------------------------------------------------------------------- maxSessions ?
        var newRefreshTokenEntity = new RefreshToken(
            oldRefreshTokenEntity.UserName,
            authGenerator.GenerateFingerprint(httpContext),
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(1),
            null,
            authGenerator.GenerateHash(newRefreshToken),
            string.Empty,
            string.Empty,
            httpContext.Connection.RemoteIpAddress?.ToString(),
            httpContext.Request.Headers.UserAgent.ToString()
        );
        await refreshTokenRepository.InsertAsync(newRefreshTokenEntity, CancellationToken.None);

        var newAccessToken = authGenerator.GenerateAccessToken(oldRefreshTokenEntity.UserName, newRefreshTokenEntity.Fingerprint);
        authGenerator.AddSecureCookie(httpContext, CookieKeyEnum.Fingerprint, newRefreshTokenEntity.Fingerprint);
        authGenerator.AddSecureCookie(httpContext, CookieKeyEnum.RefreshToken, newRefreshToken);

        return Results.Ok(new Response() { AccessToken = newAccessToken, RefreshToken = newRefreshToken });
    }


    private static async Task HandleReuseAttack(IRepository<RefreshToken> refreshTokenRepository, RefreshToken refreshTokenEntity, CancellationToken cancellationToken)
    {
        if (refreshTokenEntity.ReplacedByHash == null)
            return;

        var activeRefreshTokens = await refreshTokenRepository
            .FilterAsync(
                x => x.UserName == refreshTokenEntity.UserName && x.RevokedAt == null,
                cancellationToken
            );

        foreach (var activeRefreshToken in activeRefreshTokens)
        {
            var updatedRefreshToken = activeRefreshToken with
            {
                RevokedAt = DateTime.UtcNow,
                RevocationReason = "Reuse attack"
            };

            await refreshTokenRepository.UpdateAsync(updatedRefreshToken, CancellationToken.None);
        }
    }
}
