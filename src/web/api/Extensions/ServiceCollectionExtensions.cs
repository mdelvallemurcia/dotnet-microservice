using Api.Features.Shared;
using Api.Features.Shared.Auth;
using Asp.Versioning;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Reflection;
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

    public static IServiceCollection ConfigureBearerTokenGenerator  (this IServiceCollection services)
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
        services
            .AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitMqOptions.HostName, rabbitMqOptions.Port, "/", h =>
                    {
                        h.Username(rabbitMqOptions.UserName);
                        h.Password(rabbitMqOptions.Password);  
                    });

                    cfg.UseTimeout(c => c.Timeout = TimeSpan.FromSeconds(5));
                    cfg.UseMessageRetry(r =>
                    {
                        r.Incremental(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
                        //r.Ignore<InvalidOperationException>();
                    });

                    cfg.UseKillSwitch(options => options
                        .SetActivationThreshold(10)
                        .SetTripThreshold(0.15)
                        .SetRestartTimeout(TimeSpan.FromSeconds(30))                       
                        .SetTrackingPeriod(TimeSpan.FromSeconds(30))
                    );

                    cfg.MessageTopology.SetEntityNameFormatter(new EntityNameFormatter(rabbitMqOptions));
                    cfg.PublishTopology.BrokerTopologyOptions = PublishBrokerTopologyOptions.MaintainHierarchy;
                    cfg.DeployPublishTopology = true;

                    // Configuración automática de Exchanges y Queues
                    cfg.ConfigureEndpoints(context);
                });

            });

        return services;
    }

    public static IServiceCollection AddInternalServices(this IServiceCollection services)
    {
        //services
        //    .AddSingleton<IEventPublisher, EventPublisher>();

        //services
        //    .AddOptions<RabbitMqOptions>()
        //    .BindConfiguration(RabbitMqOptions.Section);

        return services;
    }
}