using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var rabbitmq = builder
    .AddRabbitMQ("messaging")
    .WithManagementPlugin();

builder
    .AddProject<api>("api")
    .WithReference(rabbitmq);

builder.Build().Run();
