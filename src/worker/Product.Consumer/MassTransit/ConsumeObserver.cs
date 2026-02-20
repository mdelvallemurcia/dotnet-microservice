using MassTransit;

namespace ProjectSubscriber.MassTransit;

public class ConsumeObserver : IConsumeObserver
{
    // Se ejecuta justo cuando el mensaje llega de RabbitMQ, antes de buscar al Consumer
    public Task PreConsume<T>(ConsumeContext<T> context) where T : class
    {
        Console.WriteLine($"📩 Mensaje recibido: {typeof(T).Name}");
        Console.WriteLine($"🆔 ID: {context.MessageId}");
        Console.WriteLine($"📍 RoutingKey: {context.RoutingKey()}"); // Requiere using MassTransit.RabbitMqTransport
        return Task.CompletedTask;
    }

    // Se ejecuta después de que el Consumer ha terminado con éxito
    public Task PostConsume<T>(ConsumeContext<T> context) where T : class
    {
        Console.WriteLine($"✅ Mensaje procesado con éxito: {typeof(T).Name}");
        return Task.CompletedTask;
    }

    // Se ejecuta si el Consumer lanza una excepción
    public Task ConsumeFault<T>(ConsumeContext<T> context, Exception exception) where T : class
    {
        Console.Error.WriteLine($"❌ Error procesando {typeof(T).Name}: {exception.Message}");
        return Task.CompletedTask;
    }
}
