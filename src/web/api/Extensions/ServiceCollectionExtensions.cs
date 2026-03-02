using api.MassTransit;
using api.Options;
using Api.Features.Shared.Auth;
using Asp.Versioning;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Text;

namespace Api.Extensions;

public static class ServiceCollectionExtensions
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

    public static IServiceCollection ConfigureBearerTokenGenerator(this IServiceCollection services)
    {
        services
            .AddOptions<BearerTokenOptions>()
            .BindConfiguration(BearerTokenOptions.Section);

        services.AddSingleton<IBearerTokenGenerator, BearerTokenGenerator>();            

        return services;
    }

    public static IServiceCollection ConfigureAuthentication(this IServiceCollection services, BearerTokenOptions bearerTokenOptions)
    {
        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine("Auth fail! " + context.Exception.Message);
                        return Task.CompletedTask;
                    }
                };

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = bearerTokenOptions.Issuer,
                    ValidAudience = bearerTokenOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(bearerTokenOptions.SecretKey))
                };
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
                pc.ProblemDetails.Extensions.Add("traceId", Activity.Current.TraceId.ToString());                
            });

        return services;
    }

    public static IServiceCollection ConfigureCors(this IServiceCollection services)
    {
        services
            .AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
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
                    .AddSource(Infrastructure.Repository.Const.MongoOtelSource)
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
}