using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Api.Common;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IWebHostEnvironment env)
    {
        _env = env;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unexpected error occurred: {Message}", exception.Message);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Server Error",
            Detail = "An unexpected error occurred",
        };

        if (_env.IsDevelopment())
        {
            problemDetails.Detail = exception.Message;
            problemDetails.Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}";
            problemDetails.Extensions.Add("id", Activity.Current.Id);
            problemDetails.Extensions.Add("spanId", Activity.Current.SpanId.ToString());
            problemDetails.Extensions.Add("traceId", Activity.Current.TraceId.ToString());
        }

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
