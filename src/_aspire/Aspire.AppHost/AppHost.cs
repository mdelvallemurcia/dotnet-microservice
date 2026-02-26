using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var rabbitMqUsername = builder.AddParameter("username", "guest", secret: true);
var rabbitMqPassword = builder.AddParameter("password", "guest", secret: true);
var rabbitmq = builder
    .AddRabbitMQ("messaging", rabbitMqUsername, rabbitMqPassword, 5672)
    .WithManagementPlugin()
    .WithBindMount("./rabbitmq-definitions.json", "/etc/rabbitmq/rabbitmq-definitions.json")
    .WithEnvironment("RABBITMQ_SERVER_ADDITIONAL_ERL_ARGS", "-rabbitmq_management load_definitions \"/etc/rabbitmq/rabbitmq-definitions.json\""); ;

var mongoDb = builder
    .AddMongoDB("MongoDb", 27017)
    .WithMongoExpress();

builder
    .AddProject<api>("Api")
    .WaitForStart(rabbitmq)
    .WithReference(mongoDb);

builder
    .AddProject<ProjectSubscriber>("ProjectSubscriber")
    .WaitForStart(rabbitmq)
    .WithReference(mongoDb);

builder.Build().Run();
