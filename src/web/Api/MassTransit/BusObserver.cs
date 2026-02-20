using MassTransit;

namespace api.MassTransit;

public class BusObserver : IBusObserver
{
    public Task PostStart(IBus bus, Task<BusReady> busReady)
    {
        Console.WriteLine($"🚀 RabbitMQ Conectado: {bus.Address}");
        return Task.CompletedTask;
    }

    public Task CreateFaulted(Exception exception) => Task.CompletedTask;
    public Task PostStop(IBus bus) => Task.CompletedTask;
    public Task PreStart(IBus bus) => Task.CompletedTask;
    public Task StartFaulted(IBus bus, Exception exception) => Task.CompletedTask;
    public Task PreStop(IBus bus) => Task.CompletedTask;
    public Task StopFaulted(IBus bus, Exception exception) => Task.CompletedTask;

    public void PostCreate(IBus bus) { }
    void IBusObserver.CreateFaulted(Exception exception) { }
}
