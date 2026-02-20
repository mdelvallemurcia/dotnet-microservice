using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var rabbitMqUsername = builder.AddParameter("username", "guest", secret: true);
var rabbitMqPassword = builder.AddParameter("password", "guest", secret: true);
var rabbitmq = builder
    .AddRabbitMQ("messaging", rabbitMqUsername, rabbitMqPassword, 5672)
    .WithManagementPlugin()
    .WithBindMount("./rabbitmq-definitions.json", "/etc/rabbitmq/rabbitmq-definitions.json")
    .WithEnvironment("RABBITMQ_SERVER_ADDITIONAL_ERL_ARGS", "-rabbitmq_management load_definitions \"/etc/rabbitmq/rabbitmq-definitions.json\""); ;

builder
    .AddProject<api>("Api")
    .WithReference(rabbitmq);

builder
    .AddProject<ProjectSubscriber>("ProjectSubscriber")
    .WithReference(rabbitmq);

builder.Build().Run();
