namespace Infrastructure.Repository;

public class MongoDbOptions
{
    public const string Section = "MongoDb";

    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
}
