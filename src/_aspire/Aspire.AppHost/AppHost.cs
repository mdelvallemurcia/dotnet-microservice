using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var rabbitMqUsername = builder.AddParameter("username", "guest", secret: true);
var rabbitMqPassword = builder.AddParameter("password", "guest", secret: true);
var rabbitmq = builder
    .AddRabbitMQ("messaging", rabbitMqUsername, rabbitMqPassword, 15672)
    .WithManagementPlugin();

builder
    .AddProject<api>("api")
    .WithReference(rabbitmq);

builder
    .AddProject<ProjectSubscriber>("ProjectSubscriber")
    .WithReference(rabbitmq);

builder.Build().Run();
