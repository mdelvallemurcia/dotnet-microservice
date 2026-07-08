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
    .AddMongoDB("MongoDb", 27017);
//.WithMongoExpress();

var lgtm = builder
    .AddContainer("lgtm", "grafana/otel-lgtm")
    .WithEnvironment("OTELCOL_LOG_LEVEL", "debug")
    .WithHttpEndpoint(port: 3000, targetPort: 3000, name: "grafana")
    .WithEndpoint(port: 4317, targetPort: 4317, name: "otlp-grpc")
    .WithEndpoint(port: 4318, targetPort: 4318, name: "otlp-http");

var otelEndpoint = lgtm.GetEndpoint("otlp-http");
var api = builder
    .AddProject<Api>("Api")
    .WaitForStart(rabbitmq)
    .WithReference(mongoDb)
    .WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", "http://localhost:4318")
    .WithEnvironment("OTEL_EXPORTER_OTLP_PROTOCOL", "http/protobuf")
    .WithEnvironment("OTEL_EXPORTER_OTLP_INSECURE", "true");

builder
    .AddProject<ProjectSubscriber>("ProjectSubscriber")
    .WaitFor(rabbitmq)
    .WithReference(mongoDb)
    .WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", "http://localhost:4318")
    .WithEnvironment("OTEL_EXPORTER_OTLP_PROTOCOL", "http/protobuf")
    .WithEnvironment("OTEL_EXPORTER_OTLP_INSECURE", "true");

//var frontendPath = Path.GetFullPath(Path.Combine(builder.AppHostDirectory, "../../Web/Ui"));
builder
    .AddJavaScriptApp("frontend", "../../Web/Ui", runScriptName: "dev")
    .WithReference(api)
    .WithHttpEndpoint(port: 5173, targetPort: 5174, name: "http")
    .WithEnvironment("NODE_ENV", "development")
    .WithExternalHttpEndpoints()
    .ExcludeFromManifest();

builder.Build().Run();
