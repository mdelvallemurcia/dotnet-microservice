using Api.Features.Services.Auth;
using Microsoft.AspNetCore.Http;
using Models.Entity;

namespace Api.Features.Auth.Login.v1;

internal static class Mapper
{
    internal static RefreshToken ToRefreshToken(this Request request, string refreshToken, HttpContext httpContext, IAuthFacade authGenerator)
    {
        //config
        return new(
            request.UserName,
            authGenerator.GenerateFingerprint(httpContext),
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(1),
            null,
            authGenerator.GenerateHash(refreshToken),
            string.Empty,
            string.Empty,
            httpContext.Connection.RemoteIpAddress?.ToString(),
            httpContext.Request.Headers.UserAgent.ToString()
        );
    }
}
