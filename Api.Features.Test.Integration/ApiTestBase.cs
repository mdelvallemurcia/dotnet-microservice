using Microsoft.AspNetCore.Mvc.Testing;

namespace Api.Features.Test.Integration;

public class ApiTestBase : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly HttpClient Client;
    protected readonly WebApplicationFactory<Program> Factory;

    public ApiTestBase(WebApplicationFactory<Program> factory)
    {
        Factory = factory.WithWebHostBuilder(builder =>
        {
            // Esto le dice al test: "Busca los archivos de configuración donde está la clase Program"
            builder.UseContentRoot(Directory.GetCurrentDirectory());

            //builder.ConfigureServices(services =>{});

            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.Development.json", optional: true);
            });
        });

        Client = Factory.CreateClient();
    }

}
