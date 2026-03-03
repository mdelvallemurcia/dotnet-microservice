using MassTransit;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using ProjectSubscriber.MassTransit;
using ProjectSubscriber.Options;
using System.Reflection;

namespace ProjectSubscriber.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureMassTransit(this IServiceCollection services, RabbitMqOptions rabbitMqOptions)
    {
        services
            .AddMassTransit(x =>
            {
                x.AddConsumers(Assembly.GetExecutingAssembly());

                services.AddSingleton<IConsumeObserver, ConsumeObserver>();
                x.UsingRabbitMq((context, cfg) =>
                {                    
                    cfg.Host(rabbitMqOptions.HostName, rabbitMqOptions.Port, rabbitMqOptions.Vhost, h =>
                    {
                        h.Username(rabbitMqOptions.UserName);
                        h.Password(rabbitMqOptions.Password);
                        h.Heartbeat(rabbitMqOptions.Heartbeat);
                        h.PublisherConfirmation = true;
                    });
                   
                    cfg.ReceiveEndpoint(
                        "pm.subscriber",
                        e =>
                        {
                            e.SetQuorumQueue();                 // ojo! posible procesamiento duplicado, pero garantiza alta disponibilidad y durabilidad                        
                            e.PrefetchCount = 32;               // cuántos mensajes se bajan a la RAM de la App a la vez
                            e.ConcurrentMessageLimit = 10;      // hilos                            

                            e.ConfigureConsumers(context);
                        }
                    );

                    cfg.UseTimeout(c => c.Timeout = TimeSpan.FromSeconds(5));

                    cfg.UseMessageRetry(r =>
                    {
                        r.Incremental(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
                        //r.Ignore<InvalidOperationException>();
                    });

                    cfg.UseCircuitBreaker(cb =>
                    {
                        cb.TripThreshold = 15;
                        cb.ActiveThreshold = 10;
                        cb.ResetInterval = TimeSpan.FromMinutes(5);
                    });

                    cfg.UseKillSwitch(options =>
                        options
                            .SetActivationThreshold(10)
                            .SetTripThreshold(0.15)
                            .SetRestartTimeout(TimeSpan.FromSeconds(30))
                            .SetTrackingPeriod(TimeSpan.FromSeconds(30))
                    );
                                        
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
}
