namespace Infrastructure.Repository;

public class MongoDbOptions
{
    public const string Section = "MongoDb";

    //public string ConnectionName { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    //public string UserName { get; set; } = string.Empty;
    //public string Password { get; set; } = string.Empty;

    //.AddMongoDB("database", "mongodb://localhost:27017", "AspireDb")
    //.WithEnvironment("MONGO_INITDB_ROOT_USERNAME", "admin")
    //.WithEnvironment("MONGO_INITDB_ROOT_PASSWORD", "password");
}
