namespace ProjectSubscriber.Options;

public class RabbitMqOptions
{
    public const string Section = "RabbitMq";

    public string HostName { get; set; } = string.Empty;
    public ushort Port { get; set; }
    public string Vhost { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ExchangeName { get; set; } = string.Empty;
    public string RoutingKeyFilter { get; set; } = string.Empty;
    public ushort Heartbeat { get; set; } = 30;
}
