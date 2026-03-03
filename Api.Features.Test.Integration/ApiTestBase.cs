using Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc.Testing;
using Models.Entity;
using Moq;

namespace Api.Features.Test.Integration;

public class ApiTestBase : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    internal HttpClient Client { get; set; }
    internal ApiTestBase(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            // Esto le dice al test: "Busca los archivos de configuración donde está la clase Program"
            builder.UseContentRoot(Directory.GetCurrentDirectory());

            builder.ConfigureServices(services =>
            {
                services.AddScoped(cfg => Mock.Of<IRepository<Project>>());
            });

            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.Development.json", optional: true);
            });
        });

        Client = _factory.CreateClient();
    }

}

