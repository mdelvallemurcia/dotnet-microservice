namespace Api.Features.Shared;

public class RabbitMqOptions
{
    public const string Section = "RabbitMq";

    public string HostName { get; set; } = string.Empty;
    public ushort Port { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ExchangeName { get; set; } = string.Empty;
    public string InvalidTopicQueue { get; set; } = string.Empty;
    public bool Durable { get; set; } = true;

    public Dictionary<string, string> Topics { get; set; } = new();
}
