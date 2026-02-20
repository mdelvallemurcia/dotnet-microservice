using ProjectSubscriber.Extensions;
using ProjectSubscriber.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .ConfigureMassTransit(builder.Configuration.GetSection(RabbitMqOptions.Section).Get<RabbitMqOptions>());

var host = builder.Build();

await host.RunAsync();