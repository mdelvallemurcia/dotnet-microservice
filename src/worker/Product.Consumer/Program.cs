using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using ProjectSubscriber.Extensions;
using ProjectSubscriber.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .ConfigureMassTransit(builder.Configuration.GetSection(RabbitMqOptions.Section).Get<RabbitMqOptions>()!)
    .ConfigureOpenTelemetry()
    .ConfigureHealthChecks();

builder.Logging
    .AddOpenTelemetry(logging =>
    {
        var serviceName = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME") ?? string.Empty;
        logging.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName));
        logging.IncludeFormattedMessage = true;
        logging.IncludeScopes = true;
        logging.AddOtlpExporter();
    });

builder.WebHost.ConfigureKestrel(
    serverOptions => { serverOptions.ListenAnyIP(8081); }
);

var app = builder.Build();

app.MapDefaultHealthChecks();

await app.RunAsync();