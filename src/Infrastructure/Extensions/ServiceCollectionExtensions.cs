using Infrastructure.Repository;
using Microsoft.Extensions.DependencyInjection;
using Models.Entity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;

namespace Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMongoRepository(this IServiceCollection services, MongoDbOptions mongoDbOptions)
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(BaseEntity)))
        {
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
            BsonClassMap.RegisterClassMap<BaseEntity>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(c => c.Id);
            });
        }

        services.AddSingleton<IMongoClient>(sp =>
        {
            var clientSettings = MongoClientSettings.FromConnectionString(mongoDbOptions.ConnectionString);
            clientSettings.ClusterConfigurator = cb => cb.Subscribe(new DiagnosticsActivityEventSubscriber());
            return new MongoClient(clientSettings);
        });

        services.AddScoped(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(mongoDbOptions.DatabaseName);
        });

        services.AddScoped(typeof(IRepository<>), typeof(MongoRepository<>));

        return services;
    }
}
