using System.Diagnostics;
using System.Text;
using api.AuthenticationHandlers;
using api.MassTransit;
using api.Options;
using Api.Features.Shared.Auth;
using Asp.Versioning;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Api.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureApiVersioning(this IServiceCollection services)
    {
        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                //options.ReportApiVersions = true; // Añade la versión en los headers de respuesta
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            });
        services
            .AddEndpointsApiExplorer();

        return services;
    }

    public static IServiceCollection AddInternalServices(this IServiceCollection services)
    {
        services
            .AddOptions<BearerTokenOptions>()
            .BindConfiguration(BearerTokenOptions.Section);

        services.AddSingleton<Features.Services.Auth.IAuthFacade, Features.Services.Auth.AuthFacade>();
        services.AddSingleton(TimeProvider.System); // HMAC, necessary?

        return services;
    }

    public static IServiceCollection ConfigureAuthentication(this IServiceCollection services, BearerTokenOptions bearerTokenOptions)
    {
        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = "DynamicAuth";
                options.DefaultChallengeScheme = "DynamicAuth";
            })
            .AddPolicyScheme("DynamicAuth", "Bearer or HMAC", options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    var authHeader = context.Request.Headers.Authorization.ToString();

                    if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        return JwtBearerDefaults.AuthenticationScheme;

                    if (authHeader.StartsWith("ldx ", StringComparison.OrdinalIgnoreCase))
                        return HmacAuthenticationHandler.SchemeName;

                    return JwtBearerDefaults.AuthenticationScheme;
                };
            })
            .AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        //TODO! - warning????
                        Console.WriteLine("Auth fail! " + context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    //OnTokenValidated = ctx =>
                    //{
                    //    foreach (var claim in ctx.Principal!.Claims)
                    //    {
                    //        Console.WriteLine($"{claim.Type} = {claim.Value}");
                    //    }

                    //    return Task.CompletedTask;
                    //}
                };

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = bearerTokenOptions.Issuer,

                    ValidateAudience = true,
                    ValidAudience = bearerTokenOptions.Audience,

                    ValidateLifetime = true,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(bearerTokenOptions.SecretKey)),
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha512],

                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            })
            .AddScheme<AuthenticationSchemeOptions, HmacAuthenticationHandler>(
                HmacAuthenticationHandler.SchemeName, options => { }
            );

        return services;
    }

    public static IServiceCollection ConfigureAuthorization(this IServiceCollection services)
    {
        services
            .AddAuthorizationBuilder()
            .SetFallbackPolicy(
                new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build()
            )
            .AddPolicy("RequireAdmin", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(["admin"]);
            })
            .AddPolicy("RequireReader", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(["reader", "admin"]);
            });

        return services;
    }

    public static IServiceCollection ConfigureProblemDetails(this IServiceCollection services)
    {
        services
            .AddProblemDetails(po => po.CustomizeProblemDetails = pc =>
            {

                pc.ProblemDetails.Extensions.Add("id", Activity.Current.Id);
                pc.ProblemDetails.Extensions.Add("spanId", Activity.Current.SpanId.ToString());
                if (!pc.ProblemDetails.Extensions.ContainsKey("traceId"))
                    pc.ProblemDetails.Extensions.Add("traceId", Activity.Current.TraceId.ToString());
            });

        return services;
    }

    public static IServiceCollection ConfigureCors(this IServiceCollection services, string[] allowedOrigins)
    {
        services
            .AddCors(options =>
            {
                options.AddPolicy(
                    "DefaultPolicy",
                    builder =>
                    {
                        builder
                            // Origins come from config (Cors:AllowedOrigins): dev uses localhost,
                            // prod uses the UI subdomain (e.g. https://app.midominio.com).
                            // AllowCredentials is required for the auth cookies to flow.
                            .WithOrigins(allowedOrigins)
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials();
                    }
                );
            });
        return services;
    }

    public static IServiceCollection ConfigureForwardedHeaders(this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            // Behind a TLS-terminating ingress (the subdomain prod setup), the app sees plain HTTP.
            // Honoring X-Forwarded-Proto lets Request.IsHttps reflect the real scheme, so the Secure
            // cookie flag and HTTPS redirection behave correctly.
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

            // Proxy/load-balancer IPs are not known at build time (containers/dynamic infra).
            // In a hardened deployment, restrict these to the known ingress addresses instead.
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        return services;
    }

    public static IServiceCollection ConfigureMassTransit(this IServiceCollection services, RabbitMqOptions rabbitMqOptions)
    {
        // Registrar el filtro de routing key
        services.AddSingleton<RoutingKeyFilter>();

        services
            .AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitMqOptions.HostName, rabbitMqOptions.Port, rabbitMqOptions.Vhost, h =>
                    {
                        h.Username(rabbitMqOptions.UserName);
                        h.Password(rabbitMqOptions.Password);
                        h.Heartbeat(rabbitMqOptions.Heartbeat);
                        h.PublisherConfirmation = true;
                    });

                    cfg.UseMessageRetry(r => r.Incremental(5, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5)));
                    cfg.ConnectBusObserver(new BusObserver());
                });
            });

        return services;
    }

    public static IServiceCollection ConfigureOpenTelemetry(this IServiceCollection services)
    {
        Sdk.SetDefaultTextMapPropagator(new CompositeTextMapPropagator(
            [
                new TraceContextPropagator(),
                new BaggagePropagator()
            ]
        ));

        var healthEndpointPath = "/health";     //TODO get from healthcheck lib
        var alivenessEndpointPath = "/alive";
        services
            .AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .SetSampler(new AlwaysOnSampler())
                    .AddAspNetCoreInstrumentation(tracing =>
                        tracing.Filter = context =>
                            !context.Request.Path.StartsWithSegments(healthEndpointPath)
                            && !context.Request.Path.StartsWithSegments(alivenessEndpointPath)
                    )
                    .AddHttpClientInstrumentation()
                    //.AddMongoDBInstrumentation()
                    .AddSource(nameof(MassTransit))
                    .AddSource(Infrastructure.Repository.Consts.MongoOtelSource)
                    .AddOtlpExporter();
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter(nameof(MassTransit))
                    .AddOtlpExporter();
            })
            .WithLogging();

        return services;
    }

    public static IServiceCollection ConfigureHealthChecks(this IServiceCollection services)
    {
        services
            .AddHealthChecks()
            .AddCheck(
                name: "self",
                check: () => HealthCheckResult.Healthy(),
                tags: ["live"]
            )
            .AddCheck(
                name: "masstransit",
                check: () => HealthCheckResult.Healthy(),
                tags: ["ready"]
            )
            .AddMongoDb(
                name: "mongodb",
                tags: ["ready"]
            );

        services.Configure<HealthCheckPublisherOptions>(options =>
        {
            options.Delay = TimeSpan.FromSeconds(5);
            options.Period = TimeSpan.FromSeconds(10);
        });

        services.Configure<MassTransitHostOptions>(options =>
        {
            options.WaitUntilStarted = false;
        });

        return services;
    }

    //public static IServiceCollection ConfigureBffSession(this IServiceCollection services)
    //{
    //    return services.AddSession(
    //        options =>
    //        {
    //            options.Cookie.Name = "__Host-bff-session";
    //            options.Cookie.HttpOnly = true;
    //            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    //            options.Cookie.SameSite = SameSiteMode.Strict;
    //            options.IdleTimeout = TimeSpan.FromMinutes(30);
    //        });
    //}
}
