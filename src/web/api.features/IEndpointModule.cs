using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Routing;

namespace Api.Features;

public interface IEndpointModule
{
    void MapEndpoints(IEndpointRouteBuilder app, ApiVersionSet versionSet);
}
