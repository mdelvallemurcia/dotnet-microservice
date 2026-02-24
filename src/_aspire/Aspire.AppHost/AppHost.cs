using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var rabbitMqUsername = builder.AddParameter("username", "guest", secret: true);
var rabbitMqPassword = builder.AddParameter("password", "guest", secret: true);
var rabbitmq = builder
    .AddRabbitMQ("messaging", rabbitMqUsername, rabbitMqPassword, 5672)
    .WithManagementPlugin()
    .WithBindMount("./rabbitmq-definitions.json", "/etc/rabbitmq/rabbitmq-definitions.json")
    .WithEnvironment("RABBITMQ_SERVER_ADDITIONAL_ERL_ARGS", "-rabbitmq_management load_definitions \"/etc/rabbitmq/rabbitmq-definitions.json\""); ;

//var mongoDbUsername = builder.AddParameter("username", "admin", secret: true); 
//var mongoDbPassword = builder.AddParameter("password", "password", secret: true);
var mongoDb = builder
    //.AddMongoDB("database", "mongodb://localhost:27017", "AspireDb")
    //.AddMongoDB("MongoDb", 27017, mongoDbUsername, mongoDbPassword)
    //.AddMongoDB("MongoDb", userName: mongoDbUsername, password: mongoDbPassword)
    .AddMongoDB("MongoDb", 27017);

builder
    .AddProject<api>("Api")
    .WithReference(rabbitmq)
    .WithReference(mongoDb);

builder
    .AddProject<ProjectSubscriber>("ProjectSubscriber")
    .WithReference(rabbitmq)
    .WithReference(mongoDb);

builder.Build().Run();
