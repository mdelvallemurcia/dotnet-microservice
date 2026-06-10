using Api.Features.Services.Auth;
using Microsoft.AspNetCore.Http;
using Models.Entity;

namespace Api.Features.Auth.Login.v1;

internal static class Mapper
{
    internal static RefreshToken ToRefreshToken(this Request request, string refreshToken, string fingerprintHash, HttpContext httpContext, IAuthFacade authGenerator)
    {
        return new(
            request.UserName,
            fingerprintHash,
            DateTime.UtcNow,
            authGenerator.GetRefreshTokenExpiresAt(),
            null,
            authGenerator.GenerateHash(refreshToken),
            string.Empty,
            string.Empty,
            httpContext.Connection.RemoteIpAddress?.ToString(),
            httpContext.Request.Headers.UserAgent.ToString()
        );
    }
}
