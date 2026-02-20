
using Api.Features.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ProjectSubscriber.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .ConfigureMassTransit(builder.Configuration.GetSection(RabbitMqOptions.Section).Get<RabbitMqOptions>()!)
    ;

var host = builder.Build();

await host.RunAsync();