//using Api.Features.Shared;
//using Microsoft.Extensions.Options;
//using RabbitMQ.Client;
//using System.Text.Json;

//namespace Api.Features.Services.EventPublisher;

//public class EventPublisher : IEventPublisher, IDisposable
//{
//    private readonly IConnection _rabbitMqConnection;
//    private readonly IChannel _rabbitMqChannel;
//    private readonly IOptionsMonitor<RabbitMqOptions> _optionsMonitor;

//    public EventPublisher(IConnection rabbitMqConnection, IOptionsMonitor<RabbitMqOptions> optionsMonitor)
//    {
//        _rabbitMqConnection = rabbitMqConnection;
//        _optionsMonitor = optionsMonitor;
//    }

//    public async Task PublishAsync<T>(T message)
//    {
//        var options = _optionsMonitor.CurrentValue;

//        var channel = await GetChannelAsync(options);
//        var routingKey = GetRoutingKey(message, options);

//        await channel.ExchangeDeclareAsync(exchange: options.ExchangeName, type: ExchangeType.Topic);

//        await channel.BasicPublishAsync(
//            exchange: options.ExchangeName,
//            routingKey: routingKey,
//            body: JsonSerializer.SerializeToUtf8Bytes(message));
//    }

//    private string GetRoutingKey<T>(T message, RabbitMqOptions options)
//    {
//        var topic = options.Topics?
//            .FirstOrDefault(t => t.Key.Equals(typeof(T).Name, StringComparison.OrdinalIgnoreCase))
//            .Value;

//        if (topic == null) 
//            return "error.system.database";

//        return topic;
//    }

//    private async Task<IChannel> GetChannelAsync(RabbitMqOptions options)
//    {
//        // Reutilizamos el canal si está abierto
//        if (_rabbitMqChannel is { IsOpen: true }) return _rabbitMqChannel;

//        _rabbitMqChannel = await _rabbitMqConnection.CreateChannelAsync();

//        // Declaramos el Exchange UNA SOLA VEZ al crear el canal
//        await _rabbitMqChannel.ExchangeDeclareAsync(
//            exchange: _optionsMonitor.CurrentValue.ExchangeName,
//            type: ExchangeType.Topic,
//            durable: options.Durable);

//        return _rabbitMqChannel;
//    }

//    public void Dispose()
//    {
//        //_rabbitMqChannel?.Dispose();
//    }
//}
