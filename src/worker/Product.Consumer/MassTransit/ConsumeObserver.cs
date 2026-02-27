using MassTransit;
using System.Text;

namespace ProjectSubscriber.MassTransit;

public class ConsumeObserver : IConsumeObserver
{
    public ILogger<ConsumeObserver> _logger { get; }

    public ConsumeObserver(ILogger<ConsumeObserver> logger)
    {
        _logger = logger;
    }

    // Se ejecuta justo cuando el mensaje llega de RabbitMQ, antes de buscar al Consumer
    public Task PreConsume<T>(ConsumeContext<T> context) where T : class
    {
        var sb = new StringBuilder();
        sb.AppendLine($"📩 Mensaje recibido: {typeof(T).Name}");
        sb.AppendLine($"🆔 ID: {context.MessageId}");
        sb.AppendLine($"📍 RoutingKey: {context.RoutingKey()}");

        _logger.LogDebug(sb.ToString());

        return Task.CompletedTask;
    }

    // Se ejecuta después de que el Consumer ha terminado con éxito
    public Task PostConsume<T>(ConsumeContext<T> context) where T : class
    {
        _logger.LogDebug($"✅ Mensaje procesado con éxito: {typeof(T).Name}");
        return Task.CompletedTask;
    }

    // Se ejecuta si el Consumer lanza una excepción
    public Task ConsumeFault<T>(ConsumeContext<T> context, Exception exception) where T : class
    {
        _logger.LogError($"❌ Error procesando {typeof(T).Name}: {exception.Message}");
        return Task.CompletedTask;
    }
}
