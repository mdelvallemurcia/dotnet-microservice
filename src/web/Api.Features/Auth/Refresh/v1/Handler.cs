using Api.Features.Services.Auth;
using Asp.Versioning.Builder;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Models.Entity;

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
            .ProducesValidationProblem()
            .AllowAnonymous();
    }

    private static async Task<IResult> Handle(HttpContext httpContext, IAuthFacade authGenerator, IRepository<RefreshToken> refreshTokenRepository, CancellationToken cancellationToken)
    {
        var token = authGenerator.GetSecureCookie(httpContext, CookieKeyEnum.RefreshToken);
        if (string.IsNullOrEmpty(token))
            return Results.BadRequest();

        var tokenHash = authGenerator.GenerateHash(token);
        var refreshTokens = await refreshTokenRepository.FilterAsync(w => w.Hash.Equals(tokenHash, StringComparison.Ordinal), cancellationToken);
        var existingToken = refreshTokens?.FirstOrDefault();
        if (existingToken is null)
            return Results.Unauthorized();

        if (!existingToken.IsActive)
        {
            // A revoked token presented again means the rotated chain was replayed — treat it as
            // theft and revoke the whole family. A naturally expired token is just a re-login.
            if (existingToken.IsRevoked)
                await HandleReuseAttack(refreshTokenRepository, existingToken, cancellationToken);

            return Results.Unauthorized();
        }

        var newRefreshToken = authGenerator.GenerateRefreshToken(existingToken.UserName);
        var newRefreshTokenHash = authGenerator.GenerateHash(newRefreshToken);
        var oldRefreshTokenEntity = existingToken with
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

        return Results.Ok(new Response() { AccessToken = newAccessToken });
    }


    private static async Task HandleReuseAttack(IRepository<RefreshToken> refreshTokenRepository, RefreshToken refreshTokenEntity, CancellationToken cancellationToken)
    {
        // Revoke every still-active token of the user: the chain is considered compromised.
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
