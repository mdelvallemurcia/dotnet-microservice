using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using ProjectSubscriber.Extensions;
using ProjectSubscriber.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .ConfigureMassTransit(builder.Configuration.GetSection(RabbitMqOptions.Section).Get<RabbitMqOptions>()!)
    .ConfigureOpenTelemetry();

builder.Logging
    .AddOpenTelemetry(logging =>
    {
        var serviceName = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME") ?? string.Empty;
        logging.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName));
        logging.IncludeFormattedMessage = true;
        logging.IncludeScopes = true;
        logging.AddOtlpExporter();
    });

var host = builder.Build();

await host.RunAsync();